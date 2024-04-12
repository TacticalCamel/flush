namespace Interpreter.Bytecode;

public readonly struct ObjectReference(ulong value) {
    private readonly ulong Value = value;
    
    public static implicit operator ulong(ObjectReference objectSize) {
        return objectSize.Value;
    }
    
    public static ulong MaxValue => ulong.MaxValue;
}