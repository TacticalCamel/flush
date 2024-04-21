namespace Compiler.Types;

using Interpreter.Structs;

public sealed class FieldDraft {
    public required Modifier Modifiers { get; init; }
    public required TypeNode Type { get; init; }
    public required string Name { get; init; }
}