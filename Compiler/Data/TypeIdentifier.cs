namespace Compiler.Data;

using Interpreter.Types;

/// <summary>
/// Represents an object type. This can be a non-generic type or a specific instance of a generic type.
/// </summary>
/// <param name="definition">The base type of the identifier.</param>
/// <param name="genericParameters">An array of types which are used as generic parameters.</param>
internal sealed class TypeIdentifier(TypeDefinition definition, TypeIdentifier[] genericParameters) : IEquatable<TypeIdentifier> {
    /// <summary>
    /// The corresponding base type of the identifier.
    /// </summary>
    public TypeDefinition Definition { get; } = definition;

    /// <summary>
    /// An array of types which are used as generic parameters.
    /// Empty when the type is non-generic.
    /// </summary>
    public TypeIdentifier[] GenericParameters { get; } = genericParameters;

    /// <summary>
    /// The size of the type in bytes.
    /// TODO has incorrect value for non-primitive types
    /// </summary>
    public ushort Size => Definition.Size;

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
        if (x.Definition.Id != y.Definition.Id || x.GenericParameters.Length != y.GenericParameters.Length) {
            return false;
        }

        return !x.GenericParameters.Where((t, i) => t != y.GenericParameters[i]).Any();
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
        return IsGeneric ? $"{Definition}<{string.Join(',', (IEnumerable<TypeIdentifier>)GenericParameters)}>" : Definition.Name;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Definition, GenericParameters);
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