﻿namespace Interpreter.Types;

using System.Numerics;

public sealed class TypeDefinition: IEqualityOperators<TypeDefinition, TypeDefinition, bool> {
    public required string Module { get; init; }
    public required string Name { get; init; }
    public required Modifier Modifiers { get; init; }
    public required FieldDefinition[] Fields { get; init; }
    public required MethodDefinition[] Methods { get; init; }
    public required ushort StackSize { get; init; }
    public required bool IsReference { get; init; }
    
    public static TypeDefinition Null { get; } = new() {
        Module = "core",
        Name = "null",
        Modifiers = Modifier.None,
        Fields = [],
        Methods = [],
        StackSize = 8,
        IsReference = true
    };

    public override string ToString() {
        return $"{Module}.{Name}";
    }

    public static bool operator ==(TypeDefinition? x, TypeDefinition? y) {
        return x?.Name == y?.Name && x?.Module == y?.Module;
    }

    public static bool operator !=(TypeDefinition? x, TypeDefinition? y) {
        return !(x == y);
    }

    private bool Equals(TypeDefinition other) {
        return this == other;
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is TypeDefinition other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Module, Name);
    }
}