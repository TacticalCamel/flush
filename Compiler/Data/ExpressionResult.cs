namespace Compiler.Data;

internal sealed class ExpressionResult(MemoryAddress address, TypeIdentifier type, TypeIdentifier? secondaryType = null) {
    public MemoryAddress Address { get; } = address;
    public TypeIdentifier Type { get; } = type;
    public TypeIdentifier? SecondaryType { get; } = secondaryType;

    public override string ToString() {
        return $"{Type} at {Address}";
    }
}