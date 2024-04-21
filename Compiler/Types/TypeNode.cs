namespace Compiler.Types;

public sealed class TypeNode {
    public required Guid Id { get; init; }
    public required int GenericIndex { get; init; }
    public List<TypeNode> Children { get; } = [];

    public override string ToString() {
        string name = GenericIndex < 0 ? $"ID[{Id}]" : $"GEN[{GenericIndex}]";

        if (Children.Count == 0) {
            return name;
        }

        return $"{name}<{string.Join(", ", Children)}>";
    }
}