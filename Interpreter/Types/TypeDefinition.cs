namespace Interpreter.Types;

/// <summary>
/// Represents a type.
/// </summary>
public sealed class TypeDefinition {
    /// <summary>
    /// The modifiers of the type.
    /// </summary>
    public required Modifier Modifiers { get; init; }
    
    /// <summary>
    /// The name of the type.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The fields defined in the type.
    /// </summary>
    public required FieldDefinition[] Fields { get; init; }
    
    /// <summary>
    /// The methods defined in the type.
    /// </summary>
    public required MethodDefinition[] Methods { get; init; }
    
    /// <summary>
    /// The number of bytes the type occupies on the stack.
    /// </summary>
    public required ushort StackSize { get; init; }
    
    /// <summary>
    /// Whether the type is passed by reference or value.
    /// </summary>
    public required bool IsReference { get; init; }
}