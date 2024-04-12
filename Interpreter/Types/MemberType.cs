namespace Interpreter.Types;

/// <summary>
/// Represents a type that a type member can have. 
/// </summary>
public readonly struct MemberType : IEquatable<MemberType> {
    /// <summary>
    /// The id of type definition. Empty if the type is a generic parameter.
    /// </summary>
    public readonly Guid TypeId;

    /// <summary>
    /// The index of the generic parameter this type represents. -1 if not a generic parameter.
    /// </summary>
    public readonly int GenericIndex;

    /// <summary>
    /// The generic parameters of this type.
    /// </summary>
    public readonly int[] ChildrenIds;

    /// <summary>
    /// Create a new member type.
    /// </summary>
    /// <param name="typeId">The id of type definition.</param>
    /// <param name="genericIndex">The index of the generic parameter this type represents.</param>
    /// <param name="childrenIds">The generic parameters of this type.</param>
    public MemberType(Guid typeId, int genericIndex, int[] childrenIds) {
        TypeId = typeId;
        GenericIndex = genericIndex;
        ChildrenIds = childrenIds;
    }

    public bool Equals(MemberType other) {
        return TypeId.Equals(other.TypeId) &&
               GenericIndex == other.GenericIndex &&
               ChildrenIds.Length == other.ChildrenIds.Length &&
               !ChildrenIds.Where((id, i) => id != other.ChildrenIds[i]).Any();
    }

    public override bool Equals(object? obj) {
        return obj is MemberType other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(TypeId, GenericIndex, ChildrenIds);
    }

    public static bool operator ==(MemberType left, MemberType right) {
        return left.Equals(right);
    }

    public static bool operator !=(MemberType left, MemberType right) {
        return !left.Equals(right);
    }
}