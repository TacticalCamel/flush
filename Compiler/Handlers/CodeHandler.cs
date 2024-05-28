namespace Compiler.Handlers;

using Data;
using Interpreter.Structs;

/// <summary>
/// This class manages the instructions and variables of the program.
/// </summary>
internal sealed class CodeHandler {
    /// <summary>
    /// The list of instructions that will be the code section of the program.
    /// </summary>
    private List<Instruction> Instructions { get; } = [];

    /// <summary>
    /// The current scopes in the program.
    /// </summary>
    private Stack<Scope> StackScopes { get; } = [];

    /// <summary>
    /// The current size of the stack in bytes.
    /// </summary>
    private int StackSize { get; set; }

    /// <summary>
    /// Get the final instructions of the program.
    /// </summary>
    /// <returns>An array of instructions.</returns>
    public Instruction[] GetInstructionArray() {
        // append an exit instruction to the end
        return Instructions
            .Append(new Instruction { Code = OperationCode.exit })
            .ToArray();
    }

    /// <summary>
    /// Enter a new scope.
    /// </summary>
    public void EnterScope() {
        // create a new empty scope
        Scope scope = new(StackSize);

        // add the scope to the collection
        StackScopes.Push(scope);
    }

    /// <summary>
    /// Exit from the innermost scope.
    /// </summary>
    public void ExitScope() {
        // get and remove the current scope from the collection
        Scope scope = StackScopes.Pop();

        // free all declared variables by setting the stack size back to its value before we entered this scope
        EmitPop(StackSize - scope.StackSizeBefore);
    }

    /// <summary>
    /// Create a jump in the current location.
    /// </summary>
    /// <remarks>
    /// The parameters of the jump are not assigned, can be used to jump forwards.
    /// </remarks>
    /// <returns>The handle to the jump.</returns>
    public JumpHandle CreateJumpPlaceholder() {
        // reserve an index for a future jump instruction
        Instructions.Add(default);

        // create a source handle for that index
        return new JumpHandle(Instructions.Count - 1, true);
    }

    /// <summary>
    /// Create a label in the current location.
    /// </summary>
    /// <remarks>
    /// Records the index of the current instruction in a handle, can be used to jump backwards.
    /// </remarks>
    /// <returns>The handle to the label.</returns>
    public JumpHandle CreateLabel() {
        // create a label that points to the next instruction
        return new JumpHandle(Instructions.Count, false);
    }

    /// <summary>
    /// Finishes the creation of a jump.
    /// </summary>
    /// <param name="handle">The handle to the jump.</param>
    /// <param name="hasCondition">Whether the jump is conditional.</param>
    public void FinishJump(JumpHandle handle, bool hasCondition) {
        // the operation code of the jump
        OperationCode code = hasCondition ? OperationCode.cjmp : OperationCode.jump;

        // handle is a jump instruction
        if (handle.IsSource) {
            // create a jump instruction at the recorded index, that jumps to the current location
            Instructions[handle.Index] = new Instruction {
                Code = code,
                Address = Instructions.Count
            };
        }

        // handle is a label
        else {
            // create a jump instruction at the current location, that jumps to the recorded index
            Instructions.Add(new Instruction {
                Code = code,
                Address = handle.Index
            });
        }

        // decrease stack size if the jump is conditional
        StackSize -= hasCondition ? 1 : 0;
    }

    /// <summary>
    /// Defines a variable in the current scope.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="type">The type of the variable.</param>
    /// <param name="hasValue">Whether the variable was declared with a value.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    public bool DefineVariable(string name, TypeIdentifier type, bool hasValue) {
        // variable already declared
        if (StackScopes.Any(scope => scope.DeclaredVariables.Any(x => x.Name == name))) {
            return false;
        }

        ushort typeSize = type.Definition.IsReference ? (ushort)8 : type.Size;
            
        if (!hasValue) {
            Instructions.Add(new Instruction {
                Code = OperationCode.pshz,
                Count = typeSize
            });

            StackSize += typeSize;
        }

        // create a new variable
        Variable variable = new(name, new ExpressionResult(StackSize - typeSize, type));

        // get the current scope
        Scope scope = StackScopes.Peek();

        // declare variable in current scope
        scope.DeclaredVariables.Add(variable);

        return true;
    }

    /// <summary>
    /// Find a declared variable by name.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns>The type and address of the variable if successful, null otherwise.</returns>
    public ExpressionResult? GetVariable(string name) {
        // search every variable in every scope and return one if the name matches
        return (from scope in StackScopes from variable in scope.DeclaredVariables where variable.Name == name select variable.ExpressionResult).FirstOrDefault();
    }

    /// <summary>
    /// Push bytes from the data section to the top of the stack.
    /// Increase the stack size.
    /// </summary>
    /// <param name="address">The address in the data section.</param>
    /// <param name="size">The number of bytes.</param>
    public void PushBytesFromData(int address, ushort size) {
        // do not emit when size is 0
        if (size == 0) {
            return;
        }

        // emit the instruction
        Instructions.Add(new Instruction {
            Code = OperationCode.pshd,
            Address = address,
            TypeSize = size
        });

        // increase stack size
        StackSize += size;
    }

    /// <summary>
    /// Copy bytes from the stack to the top of the stack.
    /// Increase the stack size.
    /// </summary>
    /// <param name="address">The address in the stack.</param>
    /// <param name="size">The number of bytes.</param>
    public void PushBytesFromStack(int address, ushort size) {
        // do not emit when size is 0
        if (size == 0) {
            return;
        }

        // emit the instruction
        Instructions.Add(new Instruction {
            Code = OperationCode.pshs,
            Address = address,
            TypeSize = size
        });

        // increase stack size
        StackSize += size;
    }

    /// <summary>
    /// Discard the result of an expression by removing it from the stack.
    /// </summary>
    /// <param name="type">The type of the expression.</param>
    public void DiscardExpressionResult(TypeIdentifier type) {
        EmitPop(type.Size);
    }

    /// <summary>
    /// Emit the default implementation of a binary operation.
    /// </summary>
    /// <param name="size">The size of the operands in bytes.</param>
    /// <param name="code">The code of the operation.</param>
    public void EmitDefaultBinaryOperation(ushort size, OperationCode code) {
        // emit the instruction
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        // decrease stack size by the operand size
        StackSize -= size;
    }

    /// <summary>
    /// Emit a bit shift operation.
    /// </summary>
    /// <param name="size">The size of the left operand in bytes.</param>
    /// <param name="code">The code of the operation.</param>
    public void EmitShiftOperation(ushort size, OperationCode code) {
        // emit the instruction
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        // decrease stack size by the size of the right operand, which should always be 4
        StackSize -= 4;
    }

    /// <summary>
    /// Emit a comparison operation.
    /// </summary>
    /// <param name="size">The size of the operands in bytes.</param>
    /// <param name="code">The code of the operation.</param>
    public void EmitComparisonOperation(ushort size, OperationCode code) {
        // emit the instruction
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        // decrease stack size by the size of both operands, minus 1 because the result is a bool
        StackSize -= size * 2 - 1;
    }

    /// <summary>
    /// Emit an assignment operation.
    /// </summary>
    /// <param name="size">The size of the operands in bytes.</param>
    /// <param name="address">The destination stack address.</param>
    public void EmitAssignmentOperation(ushort size, int address) {
        // emit the instruction
        Instructions.Add(new Instruction {
            Code = OperationCode.asgm,
            Address = address,
            TypeSize = size
        });

        // decrease stack size by the operand size
        StackSize -= size;
    }

    /// <summary>
    /// Emit the default implementation of an unary operation.
    /// </summary>
    /// <param name="size">The size of the operand in bytes.</param>
    /// <param name="code">The code of the operation.</param>
    public void EmitDefaultUnaryOperation(ushort size, OperationCode code) {
        // emit the instruction
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        // leave the stack size unchanged
    }

    /// <summary>
    /// Emit a cast operation.
    /// </summary>
    /// <param name="sourceSize">The size of the original type in bytes.</param>
    /// <param name="destinationSize">The size of the destination type in bytes.</param>
    /// <param name="cast">The type of the cast.</param>
    public void EmitCast(ushort sourceSize, ushort destinationSize, PrimitiveCast cast) {
        // calculate stack size change
        int difference = destinationSize - sourceSize;

        // change stack size
        StackSize += difference;

        switch (cast) {
            case PrimitiveCast.ResizeImplicit or PrimitiveCast.ResizeExplicit:
                // no size change, no cast necessary
                if (difference == 0) {
                    break;
                }

                // resize cast, pop or push bytes
                Instructions.Add(new Instruction {
                    Code = difference > 0 ? OperationCode.pshz : OperationCode.pop,
                    Count = difference > 0 ? (uint)difference : (uint)-difference
                });

                break;

            case PrimitiveCast.FloatToFloatExplicit or PrimitiveCast.FloatToFloatImplicit:
                // float to float conversion
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftof,
                    TypeSize = sourceSize,
                    SecondTypeSize = destinationSize
                });

                break;

            case PrimitiveCast.FloatToSignedExplicit:
                // float to signed conversion
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftoi,
                    TypeSize = sourceSize,
                    SecondTypeSize = destinationSize
                });

                break;

            // float to unsigned conversion
            case PrimitiveCast.FloatToUnsignedExplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftou,
                    TypeSize = sourceSize,
                    SecondTypeSize = destinationSize
                });

                break;

            // signed to float conversion
            case PrimitiveCast.SignedToFloatImplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.itof,
                    TypeSize = sourceSize,
                    SecondTypeSize = destinationSize
                });

                break;

            // unsigned to float conversion
            case PrimitiveCast.UnsignedToFloatImplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.utof,
                    TypeSize = sourceSize,
                    SecondTypeSize = destinationSize
                });

                break;
        }
    }

    /// <summary>
    /// Pop bytes from the top of the stack.
    /// Decrease the stack size.
    /// </summary>
    /// <param name="count">The number of bytes.</param>
    private void EmitPop(int count) {
        // do not emit when count is 0 or negative
        if (count <= 0) {
            return;
        }

        // emit the instruction
        Instructions.Add(new Instruction {
            Code = OperationCode.pop,
            Count = (uint)count
        });

        // decrease stack size
        StackSize -= count;
    }

    /// <summary>
    /// Emit a debug instruction that pauses the program execution.
    /// </summary>
    public void EmitDebugPause() {
        Instructions.Add(new Instruction {
            Code = OperationCode.dbug
        });
    }

    /// <summary>
    /// Represents a declared variable.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="expressionResult">The address and type of the variable.</param>
    private sealed class Variable(string name, ExpressionResult expressionResult) {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// The address and type of the variable.
        /// </summary>
        public ExpressionResult ExpressionResult { get; } = expressionResult;
    }

    /// <summary>
    /// Represents a scope in the program.
    /// </summary>
    /// <param name="stackSizeBefore">The stack size in bytes before entering the scope.</param>
    private sealed class Scope(int stackSizeBefore) {
        /// <summary>
        /// The stack size in bytes before entering the scope.
        /// </summary>
        public int StackSizeBefore { get; } = stackSizeBefore;

        /// <summary>
        /// The variable declared in this scope.
        /// Does not contain variables declared in other scopes inside this one.
        /// </summary>
        public List<Variable> DeclaredVariables { get; } = [];
    }
}