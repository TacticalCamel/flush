namespace Runtime.Core;

[Alias("i128")]
public struct U128 {
    private readonly UInt128 Value;

    private U128(UInt128 value) {
        Value = value;
    }
    
    [Internal]
    public static implicit operator U128(UInt128 value) {
        return new U128(value);
    }
}