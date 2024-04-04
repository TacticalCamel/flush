namespace Interpreter.Types;

/// <summary>
/// Represents a modifier that can be applied to a type, field or method.
/// </summary>
[Flags]
public enum Modifier {
    Private = 1 << 0,
    Internal = 1 << 1,
    Readonly = 1 << 2,
}