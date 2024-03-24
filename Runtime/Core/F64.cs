namespace Runtime.Core;

[Alias("f64")]
public struct F64 {
    private readonly double Value;

    private F64(double value) {
        Value = value;
    }
    
    [Internal]
    public static implicit operator F64(double value) {
        return new F64(value);
    }
}