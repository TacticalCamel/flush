namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction() {
    [FieldOffset(0)]
    public OperationCode OperationCode = OperationCode.Exit;
    
    [FieldOffset(1)]
    public byte FirstOperand = 1;
    
    [FieldOffset(2)]
    public byte SecondOperand = 2;
    
    [FieldOffset(3)]
    public byte TargetOperand = 3;

    [FieldOffset(0)]
    public fixed byte Bytes[4];
}