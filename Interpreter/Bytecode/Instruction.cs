namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction {
    // "real" fields
    [FieldOffset(0)]
    public OperationCode Code;
    
    [FieldOffset(1)]
    public fixed byte Data[7]; 
    
    // "utility" fields
    [FieldOffset(1)]
    public int Address;
}