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

    public MemoryAddress PushFromData(ExpressionResult expression, byte size) {
        MemoryAddress address = MemoryAddress.CreateOnStack(StackSize);

        StackSize += size;

        Instructions.Add(new Instruction {
            Code = OperationCode.pshd,
            DataAddress = (int)expression.Address.Value,
            Size = size
        });

        return address;
    }

    public MemoryAddress AddInt(byte size) {
        Instructions.Add(new Instruction {
            Code = OperationCode.addi,
            Size = size
        });

        StackSize -= size;

        return MemoryAddress.CreateOnStack(StackSize - size);
    }

    public void Extend(byte fromSize, byte toSize) {
        Instructions.Add(new Instruction {
            Code = OperationCode.pshz,
            Size = fromSize,
            ToSize = toSize
        });
    }

    public void Add(Instruction instruction) {
        Instructions.Add(instruction);
    }
}