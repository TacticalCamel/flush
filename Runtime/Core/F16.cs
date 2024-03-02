namespace Runtime.Core;

[Alias("f16")]
public struct F16 {
    private readonly Half Value;

    private F16(Half value) {
        Value = value;
    }
    
    public static implicit operator F16(Half value) {
        return new F16(value);
    }
}