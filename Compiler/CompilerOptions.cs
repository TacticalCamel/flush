namespace Compiler;

/// <summary>
/// Represents a configuration of the compiler.
/// </summary>
public sealed class CompilerOptions {
    /// <summary>
    /// Treat warnings as if they were errors.
    /// </summary>
    public bool TreatWarningsAsErrors { get; init; }

    /// <summary>
    /// Suppress one or more compiler issues. Errors can not be suppressed.
    /// </summary>
    public uint[] IgnoredIssues { get; init; } = [];
}