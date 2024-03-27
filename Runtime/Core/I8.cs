namespace Runtime.Core;

[Alias("i8")]
public struct I8 {
    private readonly sbyte Value;

    private I8(sbyte value) {
        Value = value;
    }

    [Internal]
    public static implicit operator I8(sbyte value) {
        return new I8(value);
    }
}