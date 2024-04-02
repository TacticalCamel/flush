namespace Interpreter.Types;

/// <summary>
/// Represents a modifier that can be applied to a type, field or method.
/// </summary>
[Flags]
public enum Modifier {
    None = 0,
    Private = 1
}