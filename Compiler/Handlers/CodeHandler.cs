namespace Compiler.Handlers;

using Data;
using Interpreter.Bytecode;

internal sealed class CodeHandler {
    /// <summary>
    /// The list of instructions that will be the code section of the program.
    /// </summary>
    private List<Instruction> Instructions { get; } = [];
    
    
    private uint StackSize { get; set; }

    public void PushFromData(MemoryAddress address, ushort size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.pshd,
            DataAddress = (int)address.Value,
            TypeSize = size
        });

        StackSize += size;
    }

    public void Pop(ushort size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.pop,
            TypeSize = size
        });

        StackSize -= size;
    }
    
    public void Exit() {
        Instructions.Add(new Instruction {
            Code = OperationCode.exit
        });
    }

    public MemoryAddress PrimitiveBinaryOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= size;

        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }
    
    public MemoryAddress PrimitiveUnaryOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });
        
        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }
    
    public MemoryAddress PrimitiveComparisonOperation(ushort size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= (uint)(2 * size - 1);

        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }

    public bool Cast(ushort sourceSize, ushort targetSize, PrimitiveCast cast) {
        int difference = targetSize - sourceSize;

        if (difference < 0) {
            StackSize -= (uint)-difference;
        }
        else {
            StackSize += (uint)difference;
        }

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

    public int InstructionCount => Instructions.Count;

    public Instruction[] GetInstructionArray() {
        return Instructions.ToArray();
    }

    private List<Variable> Variables { get; } = [];
    private List<(int stackLength, int stackBytes)> ScopeBoundaries { get; } = [];
    
    private class Variable(string name, ExpressionResult expressionResult) {
        public string Name { get; } = name;
        public ExpressionResult ExpressionResult { get; } = expressionResult;
    }

    public void EnterScope() {
        Console.WriteLine($"+scope {Variables.Count}");
        ScopeBoundaries.Add((Variables.Count, 0));
    }

    public int ExitScope() {
        (int stackLength, int stackBytes) index = ScopeBoundaries[^1];
        
        Console.WriteLine($"-scope {index}");
        
        ScopeBoundaries.RemoveAt(ScopeBoundaries.Count - 1);
        Variables.RemoveRange(index.stackLength, Variables.Count - 1 - index.stackLength);

        return 0;
    }

    public void DefineVariable(string name, ExpressionResult address) {
        Console.WriteLine($"Define {name} {address}");
        
        Variables.Add(new Variable(name, address));
    }

    public ExpressionResult? GetVariableAddress(string name) {
        return Variables.FirstOrDefault(x => x.Name == name)?.ExpressionResult;
    }
}