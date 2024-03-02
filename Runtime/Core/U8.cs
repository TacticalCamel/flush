namespace Runtime.Core;

[Alias("u8")]
public struct U8 {
    private readonly byte Value;

    private U8(byte value) {
        Value = value;
    }
    
    public static implicit operator U8(byte value) {
        return new U8(value);
    }
}