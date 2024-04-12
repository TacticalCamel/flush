namespace Interpreter.Bytecode;

public readonly struct ObjectSize(ushort value) {
    private readonly ushort Value = value;

    public static implicit operator ushort(ObjectSize objectSize) {
        return objectSize.Value;
    }

    public static ushort MaxValue => ushort.MaxValue;
}