namespace Compiler.Data;

/// <summary>
/// Represents a 64-bit offset in memory in a given location.
/// </summary>
/// <param name="value">The offset of the address in bytes.</param>
/// <param name="location">The location of the memory address.</param>
internal readonly struct MemoryAddress(ulong value, MemoryLocation location) : IEquatable<MemoryAddress> {
    /// <summary>
    /// The default heap reference that points to no object.
    /// </summary>
    public static MemoryAddress Null { get; } = new(0, MemoryLocation.Heap);

    /// <summary>
    /// The offset of the address in bytes.
    /// It is safe to say this 64-bit value is enough.
    /// </summary>
    public ulong Value { get; } = value;

    /// <summary>
    /// The location of the memory address.
    /// </summary>
    public MemoryLocation Location { get; } = location;

    /// <summary>
    /// Compares the equality of 2 memory addresses.
    /// </summary>
    /// <param name="x">The first address.</param>
    /// <param name="y">The second address.</param>
    /// <returns>True if the 2 addresses point to the same location and have the same offset, false otherwise.</returns>
    public static bool operator ==(MemoryAddress x, MemoryAddress y) {
        return x.Value == y.Value && x.Location == y.Location;
    }

    /// <summary>
    /// Compares the inequality of 2 memory addresses.
    /// </summary>
    /// <param name="x">The first address.</param>
    /// <param name="y">The second address.</param>
    /// <returns>True if the 2 addresses point to a different location or have a different offset, false otherwise.</returns>
    public static bool operator !=(MemoryAddress x, MemoryAddress y) {
        return !(x == y);
    }

    public bool Equals(MemoryAddress other) {
        return this == other;
    }

    public override bool Equals(object? obj) {
        return obj is MemoryAddress other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Value, Location);
    }

    public override string ToString() {
        if (this == Null) {
            return "nullref";
        }

        string location = Location switch {
            MemoryLocation.Data => "data",
            MemoryLocation.Stack => "stck",
            MemoryLocation.Heap => "heap",
            _ => "none"
        };

        return $"{location}:0x{Value:x}";
    }
}