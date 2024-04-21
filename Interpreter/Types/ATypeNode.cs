namespace Interpreter.Types;

/// <summary>
/// Represents a type that a type member can have. 
/// </summary>
public readonly struct ATypeNode : IEquatable<ATypeNode> {
    /// <summary>
    /// The id of type definition. Empty if the type is a generic parameter.
    /// </summary>
    public readonly Guid Id;

    /// <summary>
    /// The index of the generic parameter this type represents. -1 if not a generic parameter.
    /// </summary>
    public readonly int GenericIndex;

    /// <summary>
    /// Create a new member type from an existing type.
    /// </summary>
    /// <param name="id">The id of type definition.</param>
    public ATypeNode(Guid id) {
        Id = id;
        GenericIndex = -1;
    }
    
    /// <summary>
    /// Create a new member type from a generic parameter.
    /// </summary>
    /// <param name="genericIndex">The index of the generic parameter this type represents.</param>
    public ATypeNode(int genericIndex) {
        Id = default;
        GenericIndex = genericIndex;
    }

    public bool Equals(ATypeNode other) {
        return Id == other.Id && GenericIndex == other.GenericIndex;
    }

    public override bool Equals(object? obj) {
        return obj is ATypeNode other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Id, GenericIndex);
    }

    public static bool operator ==(ATypeNode left, ATypeNode right) {
        return left.Equals(right);
    }

    public static bool operator !=(ATypeNode left, ATypeNode right) {
        return !left.Equals(right);
    }
}