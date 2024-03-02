namespace Runtime.Core;

[Alias("f32")]
public struct F32 {
    private readonly float Value;

    private F32(float value) {
        Value = value;
    }
    
    public static implicit operator F32(float value) {
        return new F32(value);
    }
}