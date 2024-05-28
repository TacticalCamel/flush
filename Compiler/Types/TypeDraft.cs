namespace Compiler.Types;

using Interpreter.Structs;

internal sealed class TypeDraft {
    public required Guid Id { get; init; }
    public required Modifier Modifiers { get; init; }
    public required bool IsReference { get; init; }
    public required string Name { get; init; }
    public required string[] GenericParameterNames { get; init; }
    
    public List<FieldDraft> Fields { get; } = [];
    public List<MethodDraft> Methods { get; } = [];
}