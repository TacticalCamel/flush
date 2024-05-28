namespace Compiler.Types;

using Interpreter.Structs;

internal sealed class MethodDraft {
    public required Modifier Modifiers { get; init; }
    public required TypeNode ReturnType { get; init; }
    public required string Name { get; init; }
    public required TypeNode[] ParameterTypes { get; init; }
}