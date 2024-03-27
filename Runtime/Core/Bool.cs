namespace Runtime.Core;

[Alias("bool")]
public struct Bool {
    private readonly bool Value;

    private Bool(bool value) {
        Value = value;
    }

    [Internal]
    public static implicit operator Bool(bool value) {
        return new Bool(value);
    }
}