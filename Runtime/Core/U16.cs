namespace Runtime.Core;

[Alias("u16")]
public struct U16 {
    private readonly ushort Value;

    private U16(ushort value) {
        Value = value;
    }

    [Internal]
    public static implicit operator U16(ushort value) {
        return new U16(value);
    }
}