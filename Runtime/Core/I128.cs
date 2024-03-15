namespace Runtime.Core;

[Alias("i128")]
public struct I128 {
    private readonly Int128 Value;

    private I128(Int128 value) {
        Value = value;
    }
    
    public static implicit operator I128(Int128 value) {
        return new I128(value);
    }
}