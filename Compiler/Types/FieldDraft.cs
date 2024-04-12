namespace Compiler.Types;

using Interpreter.Types;

internal sealed class FieldDraft {
    public required Modifier Modifiers { get; init; }
    public required object? Type { get; init; }
    public required string Name { get; init; }
    public required int GenericIndex { get; init; }
    public required int Size { get; init; }
}