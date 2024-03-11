namespace Interpreter.Types;

public sealed class TypeInfo {
    public required string Module { get; init; }
    public required string Name { get; init; }
    public required MemberInfo[] Members { get; init; }
}