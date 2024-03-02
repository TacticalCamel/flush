namespace Runtime.Core;

[Alias("u32")]
public struct U32 {
    private readonly uint Value;

    private U32(uint value) {
        Value = value;
    }
    
    public static implicit operator U32(uint value) {
        return new U32(value);
    }
}