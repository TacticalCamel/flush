namespace Compiler.Data;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct MemoryAddress {
    private const ulong VALUE_MASK    = 0b0011111111111111111111111111111111111111111111111111111111111111;
    private const ulong HEAP_MASK     = 0b0000000000000000000000000000000000000000000000000000000000000000;
    private const ulong DATA_MASK     = 0b1000000000000000000000000000000000000000000000000000000000000000;
    private const ulong STACK_MASK    = 0b0100000000000000000000000000000000000000000000000000000000000000;
    private const ulong LOCATION_MASK = 0b1100000000000000000000000000000000000000000000000000000000000000;
    public static MemoryAddress Null { get; } = CreateOnHeap(0);
    
    private ulong RawValue { get; }
    private ulong Location => RawValue & LOCATION_MASK;
    public ulong Value => RawValue & VALUE_MASK;
    public bool IsInData => Location == DATA_MASK;
    public bool IsOnStack => Location == STACK_MASK;
    public bool IsOnHeap => Location == HEAP_MASK;
    
    private MemoryAddress(ulong rawValue) {
        RawValue = rawValue;
    }
    
    public static MemoryAddress CreateInData(ulong value) {
        return new MemoryAddress((value & VALUE_MASK) | DATA_MASK);
    }
    
    public static MemoryAddress CreateOnStack(ulong value) {
        return new MemoryAddress((value & VALUE_MASK) | STACK_MASK);
    }
    
    public static MemoryAddress CreateOnHeap(ulong value) {
        return new MemoryAddress((value & VALUE_MASK) | HEAP_MASK);
    }

    public static bool operator ==(MemoryAddress x, MemoryAddress y) {
        return x.RawValue == y.RawValue;
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
        return HashCode.Combine(RawValue);
    }

    public override string ToString() {
        return this == Null ? "nullptr" : $"{Location switch {DATA_MASK => "data", STACK_MASK => "stck", HEAP_MASK => "heap", _ => "err"}}:0x{Value:x}";
    }
}