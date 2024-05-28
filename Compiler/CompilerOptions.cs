namespace Compiler;

/// <summary>
/// Represents a configuration of the compiler.
/// </summary>
public sealed record CompilerOptions {
    /// <summary>
    /// Treat warnings as if they were errors.
    /// </summary>
    public required bool WarningsAsErrors { get; init; }

    /// <summary>
    /// Suppress one or more compiler issues. Errors can not be suppressed.
    /// </summary>
    public required uint[] SuppressIssues { get; init; }
}