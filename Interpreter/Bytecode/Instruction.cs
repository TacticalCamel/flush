namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction {
    [FieldOffset(0)]
    public OperationCode OperationCode;
    
    [FieldOffset(1)]
    public int Address;
}