namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction {
    // real fields

    [FieldOffset(0)]
    public fixed byte Data[8];

    [FieldOffset(7)]
    public OperationCode Code;

    // union fields

    [FieldOffset(0)]
    public int DataAddress;
    
    [FieldOffset(0)]
    public int ReturnCode;
    
    [FieldOffset(0)]
    public ushort SecondTypeSize;

    [FieldOffset(4)]
    public ushort TypeSize;
}