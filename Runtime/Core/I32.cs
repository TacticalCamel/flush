namespace Runtime.Core;

[Alias("i32")]
public struct I32 {
    private readonly int Value;

    private I32(int value) {
        Value = value;
    }

    [Internal]
    public static implicit operator I32(int value) {
        return new I32(value);
    }

    public static I32 operator +(I32 x, I32 y) => x.Value + y.Value;
    public static I32 operator -(I32 x, I32 y) => x.Value - y.Value;
    public static I32 operator *(I32 x, I32 y) => x.Value * y.Value;
    public static I32 operator /(I32 x, I32 y) => x.Value / y.Value;
}