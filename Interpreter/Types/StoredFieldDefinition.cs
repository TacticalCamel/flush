namespace Interpreter.Types;

using Structs;

/// <summary>
/// Represents a defined field.
/// </summary>
public sealed class StoredFieldDefinition {
    /// <summary>
    /// The modifiers of the field.
    /// </summary>
    public required Modifier Modifiers { get; init; }
    
    /// <summary>
    /// The type of the field.
    /// </summary>
    public required TypeTree Type { get; init; }
    
    /// <summary>
    /// The name of the field.
    /// </summary>
    public required string Name { get; init; }
}