namespace Interpreter.Types;

/// <summary>
/// Represents a field in a type definition.
/// </summary>
public sealed class FieldDefinition {
    /// <summary>
    /// The modifiers of the field.
    /// </summary>
    public required Modifier Modifiers { get; init; }
    
    /// <summary>
    /// The name of the field.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The type of the field.
    /// </summary>
    public required TypeDefinition Type { get; init; }
    
    /// <summary>
    /// The offset of the first byte of the field from the first byte of the type.
    /// </summary>
    public required ushort Offset { get; init; }
}