namespace Common; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public sealed class AliasAttribute : Attribute {
    public string Name { get; }

    public AliasAttribute(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException("Alias cannot be null or whitespace");
        }

        Name = name;
    }
}