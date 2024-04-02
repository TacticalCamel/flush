namespace Interpreter.Types;

using Bytecode;

/// <summary>
/// Represent a method in a type definition.
/// </summary>
public sealed class MethodDefinition {
    /// <summary>
    /// The modifiers of the method.
    /// </summary>
    public required Modifier Modifiers { get; init; }
    
    /// <summary>
    /// The name of the method.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The return type of the method. Null when the return type is void.
    /// </summary>
    public required TypeDefinition? ReturnType { get; init; }
    
    /// <summary>
    /// The types of the method parameters in order.
    /// </summary>
    public required TypeDefinition[] ParameterTypes { get; init; }
    
    /// <summary>
    /// The instructions of the method.
    /// </summary>
    public required Instruction[] Instructions { get; init; }
}