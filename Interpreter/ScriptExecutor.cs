namespace Interpreter;

using Serialization;
using Bytecode;

public sealed unsafe class ScriptExecutor {
    private readonly ReadOnlyMemory<byte> Data;
    private readonly ReadOnlyMemory<Instruction> Instructions;
    private readonly byte[] Stack = new byte[1 << 20];
    private int InstructionPtr;
    private int StackPtr;

    public ScriptExecutor(Script script) {
        Data = script.Data;
        Instructions = script.Instructions;
    }

    public void Run() {
        while (InstructionPtr < Instructions.Length) {
            Instruction i = Instructions.Span[InstructionPtr];

            switch (i.Code) {
                case OperationCode.pshd:
                    ReadOnlySpan<byte> data = Data.Span[i.DataAddress..(i.DataAddress + i.Size)];

                    data.CopyTo(Stack.AsSpan(StackPtr));
                    StackPtr += i.Size;

                    Console.WriteLine($"{nameof(OperationCode.pshd)} addr=0x{i.DataAddress:X} size={i.Size}\n    {ToString()}\n");
                    break;

                case OperationCode.addi:
                    fixed (byte* bytePtr = &Stack[StackPtr]) {
                        switch (i.Size) {
                            case 1: 
                                *(bytePtr - 2) = (byte)(*(bytePtr - 1) + *(bytePtr - 2));
                                break;
                            case 2: {
                                ushort* ptr = (ushort*)bytePtr;
                                *(ptr - 2) = (ushort)(*(ptr - 1) + *(ptr - 2));
                                break;
                            }
                            case 4: {
                                uint* ptr = (uint*)bytePtr;
                                *(ptr - 2) = *(ptr - 1) + *(ptr - 2);
                                break;
                            }
                            case 8: {
                                ulong* ptr = (ulong*)bytePtr;
                                *(ptr - 2) = *(ptr - 1) + *(ptr - 2);
                                break;
                            }
                            case 16: {
                                UInt128* ptr = (UInt128*)bytePtr;
                                *(ptr - 2) = *(ptr - 1) + *(ptr - 2);
                                break;
                            }
                        }
                    }
                    
                    StackPtr -= i.Size;

                    Console.WriteLine($"{nameof(OperationCode.addi)} {i.Size}\n    {ToString()}\n");
                    break;

                case OperationCode.extd:
                    byte difference = (byte)(i.ToSize - i.Size);

                    Stack.AsSpan(StackPtr..(StackPtr + difference)).Clear();
                    StackPtr += difference;

                    Console.WriteLine($"{nameof(OperationCode.extd)} {i.Size}->{i.ToSize}\n    {ToString()}\n");
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