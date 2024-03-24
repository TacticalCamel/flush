namespace Compiler.Handlers;

using Data;
using System.Collections;
using Interpreter.Bytecode;

internal sealed class InstructionHandler: IEnumerable<Instruction> {
    private List<Instruction> Instructions { get; } = [];
    
    public IEnumerator<Instruction> GetEnumerator() {
        return Instructions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void PushFromData(MemoryAddress address, int size) {
        Instructions.Add(new Instruction{Code = OperationCode.PushFromData, DataAddress = (int)address.Value, Size = size});
    }

    public void AddInt(int size) {
        Instructions.Add(new Instruction{Code = OperationCode.AddInt, Size = size});
    }
}