namespace Interpreter.Types;

public sealed class FieldDefinition {
    public required Modifier Modifiers { get; init; }
    public required ushort Offset { get; init; }
    public required TypeDefinition Type { get; init; }
}