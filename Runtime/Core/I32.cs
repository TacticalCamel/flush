namespace Runtime.Core;

[Alias("i32")]
public struct I32 {
    private readonly int Value;

    private I32(int value) {
        Value = value;
    }
    
    public static implicit operator I32(int value) {
        return new I32(value);
    }
}