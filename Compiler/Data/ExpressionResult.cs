namespace Compiler.Data;

internal sealed class ExpressionResult(MemoryAddress address, TypeIdentifier type) {
    public MemoryAddress Address { get; } = address;
    public TypeIdentifier Type { get; } = type;

    public override string ToString() {
        return $"{Type} at {Address}";
    }
}