namespace Runtime.Core;

[Alias("bool")]
public struct Bool {
    private readonly byte Value;

    private Bool(bool value) {
        Value = value ? (byte)1 : (byte)0;
    }

    [Internal]
    public static implicit operator Bool(bool value) {
        return new Bool(value);
    }
}