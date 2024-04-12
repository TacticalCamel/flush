namespace Compiler.Types;

using Interpreter.Types;

internal sealed class TypeDraft {
    public required Modifier Modifiers { get; init; }
    public required bool IsReference { get; init; }
    public required string Name { get; init; }
    public required string[] GenericParameterNames { get; init; }
    
    public List<FieldDraft> Fields { get; } = [];
    public List<MethodDraft> Methods { get; } = [];
    public List<MemberType> MemberTypes { get; } = [];
    
    public bool InProgress { get; set; }
    public bool IsComplete { get; set; }
}