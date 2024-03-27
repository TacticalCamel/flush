namespace Runtime.Core;

[Alias("i64")]
public struct I64 {
    private readonly long Value;

    private I64(long value) {
        Value = value;
    }

    [Internal]
    public static implicit operator I64(long value) {
        return new I64(value);
    }
}