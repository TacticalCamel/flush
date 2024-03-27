namespace Compiler.Data;

using Interpreter.Types;

internal sealed class TypeIdentifier(TypeInfo type, TypeIdentifier[] genericParameters) {
    private TypeInfo Type { get; } = type;

    public TypeIdentifier[] GenericParameters { get; } = genericParameters;
    public byte Size => Type.Size;
    public bool IsGeneric => GenericParameters.Length > 0;

    public override string ToString() {
        return IsGeneric ? $"{Type}<{string.Join(',', (IEnumerable<TypeIdentifier>)GenericParameters)}>" : Type.ToString();
    }

    public static bool operator ==(TypeIdentifier x, TypeIdentifier y) {
        if (x.Type != y.Type || x.GenericParameters.Length != y.GenericParameters.Length) {
            return false;
        }

        for (int i = 0; i < x.GenericParameters.Length; i++) {
            if (x.GenericParameters[i] != y.GenericParameters[i]) {
                return false;
            }
        }

        return true;
    }

    public static bool operator !=(TypeIdentifier x, TypeIdentifier y) {
        return !(x == y);
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is TypeIdentifier other && this == other;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Type, GenericParameters);
    }
}