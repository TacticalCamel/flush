namespace Interpreter.Structs;

public readonly struct DataAddress(int value) : IEquatable<DataAddress> {
    private readonly int Value = value < 0 ? -1 : value;

    public static readonly DataAddress Undefined = new(-1);

    public static implicit operator int(DataAddress address) {
        return address.Value;
    }

    public static bool operator ==(DataAddress x, DataAddress y) {
        return x.Value == y.Value;
    }

    public static bool operator !=(DataAddress x, DataAddress y) {
        return !(x == y);
    }
    
    public bool Equals(DataAddress other) {
        return this == other;
    }

    public override bool Equals(object? obj) {
        return obj is DataAddress other && Equals(other);
    }

    public override int GetHashCode() {
        return Value;
    }

    public override string ToString() {
        return this == Undefined ? "<undefined>" : Value.ToString();
    }
}