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
            Size = size
        });

        StackSize += size;
    }

    public MemoryAddress AddInteger(byte size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.addi,
            Size = size
        });

        StackSize -= size;

        return new MemoryAddress(StackSize - size, MemoryLocation.Stack);
    }

    public void Add(Instruction instruction) {
        Instructions.Add(instruction);
    }
    
    public bool Cast(TypeIdentifier sourceType, TypeIdentifier targetType, PrimitiveCast cast) {
        switch(cast) {
            case PrimitiveCast.NotRequired: {
                return true;
            }
            case PrimitiveCast.ResizeImplicit: {
                int difference = targetType.Size - sourceType.Size;

                Instructions.Add(new Instruction {
                    Code = difference > 0 ? OperationCode.pshz : OperationCode.pop,
                    Size = Math.Abs(difference)
                });
                
                return true;
            }
            case PrimitiveCast.ResizeExplicit: {
                int difference = targetType.Size - sourceType.Size;

                Instructions.Add(new Instruction {
                    Code = difference > 0 ? OperationCode.pshz : OperationCode.pop,
                    Size = Math.Abs(difference)
                });
                
                return true;
            }
            default:
                return false;
        }
    }
}