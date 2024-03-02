namespace Runtime.Core;

[Alias("u64")]
public struct U64 {
    private readonly ulong Value;

    private U64(ulong value) {
        Value = value;
    }
    
    public static implicit operator U64(ulong value) {
        return new U64(value);
    }
}