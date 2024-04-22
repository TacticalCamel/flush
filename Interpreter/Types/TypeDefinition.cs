namespace Interpreter.Types;

using Structs;

/// <summary>
/// Represents a defined type.
/// </summary>
public sealed class TypeDefinition {
    /// <summary>
    /// The unique identifier of the type.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The modifiers of the type.
    /// </summary>
    public required Modifier Modifiers { get; init; }

    /// <summary>
    /// Whether the type is passed by reference or value.
    /// </summary>
    /// <remarks>
    /// Value types are stored in the stack directly.
    /// Reference types are stored on the heap and a pointer to the value is stored on the stack.
    /// </remarks>
    public required bool IsReference { get; init; }

    /// <summary>
    /// The name of the type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The number of generic parameters the type has.
    /// 0 for non-generic types.
    /// </summary>
    public required int GenericParameterCount { get; init; }

    /// <summary>
    /// The fields defined in the type.
    /// </summary>
    public required StoredFieldDefinition[] Fields { get; init; }

    /// <summary>
    /// The methods defined in the type.
    /// </summary>
    public required StoredMethodDefinition[] Methods { get; init; }

    /// <summary>
    /// The size of the type in bytes when stored by value.
    /// </summary>
    public required ushort Size { get; init; }
}