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
    /// Whether the type is passed by reference or value.
    /// </summary>
    /// <remarks>
    /// A value type's value is stored in the stack directly.
    /// A reference type's value is stored on the heap and a pointer to the value is stored on the stack.
    /// </remarks>
    public required bool IsReference { get; init; }
    
    /// <summary>
    /// The name of the type.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The number of generic parameters the type has.
    /// Has a value of 0 for non-generic types.
    /// </summary>
    public required int GenericParameterCount { get; init; }
    
    /// <summary>
    /// The fields defined in the type.
    /// </summary>
    public required FieldDefinition[] Fields { get; init; }
    
    /// <summary>
    /// The methods defined in the type.
    /// </summary>
    public required MethodDefinition[] Methods { get; init; }
    
    /// <summary>
    /// The index of the generic parameter type in the containing type.
    /// Has a value of -1 when the type is fixed.
    /// </summary>
    public required int GenericIndex { get; init; }
    
    /// <summary>
    /// The size of the type in bytes.
    /// Has a value of 0 when the type is not fixed.
    /// </summary>
    public required ushort StackSize { get; init; }
}