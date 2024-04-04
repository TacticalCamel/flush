namespace Compiler.Types;

using Interpreter.Types;

internal sealed class TypeDraft {
    public required Modifier Modifiers { get; init; }
    public required bool IsReference { get; init; }
    public required string Name { get; init; }
    public ushort? Size { get; set; }
}