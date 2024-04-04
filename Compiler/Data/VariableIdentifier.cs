namespace Compiler.Data;

internal sealed class VariableIdentifier(TypeIdentifier type, string name) {
    public TypeIdentifier Type { get; } = type;
    public string Name { get; } = name;
}