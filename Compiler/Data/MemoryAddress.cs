namespace Compiler.Data;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct MemoryAddress {
    public const byte DATA = 0;
    public const byte STACK = 1;
    public const byte HEAP = 2;

    public static readonly MemoryAddress NULL = InHeap(0);

    public readonly byte Location;
    public readonly ulong Value;

    private MemoryAddress(byte location, ulong value) {
        Location = location;
        Value = value;
    }
    
    public static MemoryAddress InData(ulong value) {
        return new MemoryAddress(DATA, value);
    }
    
    public static MemoryAddress InStack(ulong value) {
        return new MemoryAddress(STACK, value);
    }
    
    public static MemoryAddress InHeap(ulong value) {
        return new MemoryAddress(HEAP, value);
    }

    public static bool operator ==(MemoryAddress x, MemoryAddress y) {
        return x.Value == y.Value && x.Location == y.Location;
    }
    
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
        return HashCode.Combine(Location, Value);
    }

    public override string ToString() {
        return this == NULL ? "nullptr" : $"{Location switch { DATA => "data", STACK => "stck", HEAP => "heap", _ => "inva" }}:0x{Value:x}";
    }
}