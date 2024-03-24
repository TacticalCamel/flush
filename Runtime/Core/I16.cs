namespace Runtime.Core;

[Alias("i16")]
public struct I16 {
    private readonly short Value;

    private I16(short value) {
        Value = value;
    }

    [Internal]
    public static implicit operator I16(short value) {
        return new I16(value);
    }
}