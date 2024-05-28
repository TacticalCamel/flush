namespace Interpreter.Types;

using Structs;

/// <summary>
/// Represent a defined method.
/// </summary>
public sealed class StoredMethodDefinition {
    /// <summary>
    /// The modifiers of the method.
    /// </summary>
    public required Modifier Modifiers { get; init; }
    
    /// <summary>
    /// The name of the method.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The return type of the method.
    /// </summary>
    public required TypeTree ReturnType { get; init; }
    
    /// <summary>
    /// The types of the method parameters in order.
    /// </summary>
    public required TypeTree[] ParameterTypes { get; init; }
    
    /// <summary>
    /// The instructions of the method.
    /// </summary>
    public required Instruction[] Instructions { get; init; }
}