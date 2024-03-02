namespace Runtime.Core;

[Alias("char")]
public struct Char {
    private readonly char Value;
    
    private Char(char value) {
        Value = value;
    }
    
    public static implicit operator Char(char value) {
        return new Char(value);
    }
}