namespace Compiler.Handlers;

using Data;
using Interpreter.Bytecode;

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
    /// Whether the program contains any instructions.
    /// </summary>
    public bool HasInstructions => Instructions.Count > 0;

    /// <summary>
    /// Enter a new scope.
    /// </summary>
    public void EnterScope() {
        Scope scope = new(StackSize);

        StackScopes.Push(scope);
    }

    /// <summary>
    /// Exit from the innermost scope.
    /// Set the stack size back to the value it had before entering the scope.
    /// </summary>
    public void ExitScope() {
        Scope scope = StackScopes.Pop();

        PopBytes(StackSize - scope.StackSizeBefore);
    }

    public bool DefineVariable(string name, TypeIdentifier type) {
        // variable already declared
        if (StackScopes.Any(scope => scope.DeclaredVariables.Any(x => x.Name == name))) {
            return false;
        }

        Variable variable = new(name, new ExpressionResult(new MemoryAddress((ulong)StackSize, MemoryLocation.Stack), type));
        Scope scope = StackScopes.Peek();
        
        scope.DeclaredVariables.Add(variable);

        return true;
    }

    public ExpressionResult? GetVariable(string name) {
        return StackScopes
            .Select(scope => scope.DeclaredVariables.FirstOrDefault(x => x.Name == name)?.ExpressionResult)
            .FirstOrDefault();
    }

    public JumpHandle CreateJumpPlaceholder() {
        Instructions.Add(new Instruction());

        return new JumpHandle(Instructions.Count - 1);
    }

    public void ConditionalJump(JumpHandle handle) {
        Instructions[handle.Index] = new Instruction {
            Code = OperationCode.cjmp,
            Address = Instructions.Count
        };

        StackSize--;
    }
    
    public void Jump(JumpHandle handle) {
        Instructions[handle.Index] = new Instruction {
            Code = OperationCode.jump,
            Address = Instructions.Count
        };
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
        
        Instructions.Add(new Instruction {
            Code = OperationCode.pshd,
            Address = address,
            TypeSize = size
        });

        StackSize += size;
    }

    /// <summary>
    /// Pop bytes from the top of the stack.
    /// Decrease the stack size.
    /// </summary>
    /// <param name="count">The number of bytes.</param>
    public void PopBytes(int count) {
        // do not emit when count is 0 or negative
        if (count <= 0) {
            return;
        }

        Instructions.Add(new Instruction {
            Code = OperationCode.pop,
            Count = count
        });

        StackSize -= count;
    }

    public MemoryAddress PrimitiveBinaryOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= size;

        return new MemoryAddress((ulong)(StackSize - size), MemoryLocation.Stack);
    }

    public MemoryAddress PrimitiveUnaryOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        return new MemoryAddress((ulong)(StackSize - size), MemoryLocation.Stack);
    }

    public MemoryAddress PrimitiveShiftOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= 4;
        
        return new MemoryAddress((ulong)(StackSize - size), MemoryLocation.Stack);
    }
    
    public MemoryAddress PrimitiveComparisonOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= size * 2 - 1;
        
        return new MemoryAddress((ulong)(StackSize - size), MemoryLocation.Stack);
    }

    public bool Cast(ushort sourceSize, ushort targetSize, PrimitiveCast cast) {
        int difference = targetSize - sourceSize;

        StackSize += difference;

        switch (cast) {
            case PrimitiveCast.NotRequired:
                return true;

            case PrimitiveCast.ResizeImplicit or PrimitiveCast.ResizeExplicit:

                if (sourceSize == targetSize) {
                    return true;
                }

                Instructions.Add(new Instruction {
                    Code = difference > 0 ? OperationCode.pshz : OperationCode.pop,
                    TypeSize = (ushort)Math.Abs(difference)
                });

                return true;

            case PrimitiveCast.FloatToFloatExplicit or PrimitiveCast.FloatToFloatImplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftof,
                    TypeSize = sourceSize,
                    SecondTypeSize = targetSize
                });

                return true;

            case PrimitiveCast.FloatToSignedExplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftoi,
                    TypeSize = sourceSize,
                    SecondTypeSize = targetSize
                });

                return true;

            case PrimitiveCast.FloatToUnsignedExplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.ftou,
                    TypeSize = sourceSize,
                    SecondTypeSize = targetSize
                });

                return true;

            case PrimitiveCast.SignedToFloatImplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.itof,
                    TypeSize = sourceSize,
                    SecondTypeSize = targetSize
                });

                return true;

            case PrimitiveCast.UnsignedToFloatImplicit:
                Instructions.Add(new Instruction {
                    Code = OperationCode.utof,
                    TypeSize = sourceSize,
                    SecondTypeSize = targetSize
                });

                return true;

            default:
                return false;
        }
    }
    
    public Instruction[] GetInstructionArray() {
        return Instructions
            .Append(new Instruction{Code = OperationCode.exit})
            .ToArray();
    }

    private sealed class Variable(string name, ExpressionResult expressionResult) {
        public string Name { get; } = name;
        public ExpressionResult ExpressionResult { get; } = expressionResult;
    }

    private sealed class Scope(int stackSizeBefore) {
        public int StackSizeBefore { get; } = stackSizeBefore;
        public List<Variable> DeclaredVariables { get; } = [];
    }
}