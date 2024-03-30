namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction {
    // real fields

    [FieldOffset(0)]
    public fixed byte Data[15];

    [FieldOffset(15)]
    public OperationCode Code;

    // union fields

    [FieldOffset(0)]
    public int DataAddress;

    [FieldOffset(4)]
    public int TypeSize;

    [FieldOffset(8)]
    public int SecondTypeSize;
}