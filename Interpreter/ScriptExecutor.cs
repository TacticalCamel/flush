namespace Interpreter;

using Serialization;
using Bytecode;

public sealed unsafe class ScriptExecutor(Script script) {
    private readonly ReadOnlyMemory<byte> Data = script.Data;
    private readonly ReadOnlyMemory<Instruction> Instructions = script.Instructions;
    private readonly byte[] Stack = new byte[1 << 20];
    private int InstructionPtr;
    private int StackPtr;

    public void Run() {
        while (InstructionPtr < Instructions.Length) {
            Instruction i = Instructions.Span[InstructionPtr];
            
            switch (i.Code) {
                case OperationCode.PushFromData:
                    ReadOnlySpan<byte> data = Data.Span[i.DataAddress..(i.DataAddress + i.Size)];
                    
                    data.CopyTo(Stack.AsSpan(StackPtr));
                    StackPtr += i.Size;
                    
                    Console.WriteLine($"{nameof(OperationCode.PushFromData)} addr=0x{i.DataAddress:X} size={i.Size} -> {ToString()}");
                    break;
                
                case OperationCode.AddInt:
                    fixed (byte* ptr = &Stack[StackPtr]) {
                        byte a = *(ptr - 1);
                        byte b = *(ptr - 2);

                        *(ptr - 2) = (byte)(a + b);
                        
                        StackPtr -= i.Size;
                    }
                    
                    Console.WriteLine($"{nameof(OperationCode.AddInt)} {i.Size} -> {ToString()}");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            InstructionPtr++;
        }
    }

    public override string ToString() {
        return $"Stack: {string.Join(" ", Stack.AsSpan(..StackPtr).ToArray().Select(x => $"{x:X2}"))}";
    }
}