namespace Compiler.Data;

using Interpreter.Types;

internal sealed class TypeIdentifier(TypeInfo type, TypeIdentifier[] genericParameters) {
    public static TypeIdentifier Null { get; } = new(TypeInfo.Null, []);
    
    public TypeInfo Type { get; } = type;
    public TypeIdentifier[] GenericParameters { get; } = genericParameters;

    public bool IsGeneric => GenericParameters.Length > 0;

    public override string ToString() {
        return IsGeneric ? $"{Type}<{string.Join(',', (IEnumerable<TypeIdentifier>)GenericParameters)}>" : Type.ToString();
    }
}