namespace Compiler.Handlers;

using Data;
using System.Collections;
using Interpreter.Bytecode;

internal sealed class InstructionHandler : IEnumerable<Instruction> {
    private List<Instruction> Instructions { get; } = [];
    private uint StackSize { get; set; }

    public IEnumerator<Instruction> GetEnumerator() {
        return Instructions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void PushFromData(MemoryAddress address, byte size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.pshd,
            DataAddress = (int)address.Value,
            TypeSize = size
        });

        StackSize += size;
    }

    public void Pop(byte size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.pop,
            TypeSize = size
        });

        StackSize -= size;
    }

    public MemoryAddress PrimitiveBinaryOperation(byte size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= size;

        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }
    
    public MemoryAddress PrimitiveComparisonOperation(byte size, OperationCode code) {
        Instructions.Add(new Instruction {
            Code = code,
            TypeSize = size
        });

        StackSize -= (uint)(2 * size - 1);

        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }

    public bool Cast(int sourceSize, int targetSize, PrimitiveCast cast) {
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
                    TypeSize = Math.Abs(difference)
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
}