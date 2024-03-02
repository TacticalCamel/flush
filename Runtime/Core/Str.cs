namespace Runtime.Core;

[Alias("str")]
public class Str {
    private readonly string Value;

    private Str(string value) {
        Value = value;
    }
    
    public static implicit operator Str(string value) {
        return new Str(value);
    }
}