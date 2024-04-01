namespace Interpreter.Types;

public sealed class MethodDefinition {
    public required Modifier Modifiers { get; init; }
    public required TypeDefinition ReturnType { get; init; }
    public required TypeDefinition[] ParameterTypes { get; init; }
}