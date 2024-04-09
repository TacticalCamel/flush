namespace Compiler.Data;

using Interpreter.Types;

/// <summary>
/// Represents an object type. This can be a non-generic type or a specific instance of a generic type.
/// </summary>
/// <param name="baseType">The base type of the identifier.</param>
/// <param name="genericParameters">An array of types which are used as generic parameters.</param>
internal sealed class TypeIdentifier(TypeDefinition baseType, TypeIdentifier[] genericParameters) : IEquatable<TypeIdentifier> {
    /// <summary>
    /// The corresponding base type of the identifier.
    /// </summary>
    public TypeDefinition BaseType { get; } = baseType;

    /// <summary>
    /// An array of types which are used as generic parameters.
    /// Empty when the type is non generic.
    /// </summary>
    public TypeIdentifier[] GenericParameters { get; } = genericParameters;

    /// <summary>
    /// The size of the type in bytes.
    /// TODO has incorrect value for non-primitive types
    /// </summary>
    public ushort Size => BaseType.StackSize;

    /// <summary>
    /// True if the type has at least 1 generic parameter, false otherwise.
    /// </summary>
    public bool IsGeneric => GenericParameters.Length > 0;

    /// <summary>
    /// Compares the equality of 2 type identifiers.
    /// </summary>
    /// <param name="x">The first type identifier.</param>
    /// <param name="y">The second type identifier.</param>
    /// <returns>True if the type and generic parameters match, false otherwise.</returns>
    public static bool operator ==(TypeIdentifier x, TypeIdentifier y) {
        if (x.BaseType != y.BaseType || x.GenericParameters.Length != y.GenericParameters.Length) {
            return false;
        }

        for (int i = 0; i < x.GenericParameters.Length; i++) {
            if (x.GenericParameters[i] != y.GenericParameters[i]) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Compares the inequality of 2 type identifiers.
    /// </summary>
    /// <param name="x">The first type identifier.</param>
    /// <param name="y">The second type identifier.</param>
    /// <returns>True if the types are different or the generic parameters don't match, false otherwise.</returns>
    public static bool operator !=(TypeIdentifier x, TypeIdentifier y) {
        return !(x == y);
    }

    public override string ToString() {
        return IsGeneric ? $"{BaseType}<{string.Join(',', (IEnumerable<TypeIdentifier>)GenericParameters)}>" : BaseType.Name;
    }

    public override int GetHashCode() {
        return HashCode.Combine(BaseType, GenericParameters);
    }

    public bool Equals(TypeIdentifier? other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return this == other;
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is TypeIdentifier other && Equals(other);
    }
}