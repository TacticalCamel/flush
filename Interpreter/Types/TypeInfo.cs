namespace Interpreter.Types;

public sealed class TypeInfo {
    public static TypeInfo Null { get; } = new() {
        Module = "core",
        Name = "null",
        Members = []
    };

    public required string Module { get; init; }
    public required string Name { get; init; }
    public required MemberInfo[] Members { get; init; }
    
    public override string ToString() {
        return $"{Module}.{Name}";
    }

    public static bool operator ==(TypeInfo x, TypeInfo y) {
        return x.Name == y.Name && x.Module == y.Module;
    }

    public static bool operator !=(TypeInfo x, TypeInfo y) {
        return !(x == y);
    }

    private bool Equals(TypeInfo other) {
        return this == other;
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is TypeInfo other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Module, Name);
    }
}