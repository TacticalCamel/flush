namespace Runtime.Internal;

/// <summary>
/// Change the display name of a type
/// </summary>
/// <param name="name">The new name is the type</param>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
public sealed class AliasAttribute(string name): Attribute{
    public string Name { get; } = name;
}