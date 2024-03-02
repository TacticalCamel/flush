namespace Runtime.Internal;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
public class AliasAttribute(string name): Attribute{
    public string Name { get; } = name;
}