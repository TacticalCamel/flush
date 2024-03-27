namespace Compiler.Analysis;

/// <summary>
/// Represents the severity of an issue.
/// </summary>
internal enum Severity {
    /// <summary>
    /// A suggestion to improve the code.
    /// </summary>
    Hint,

    /// <summary>
    /// An issue that does not prevent compilation, but could cause unintended behaviour during runtime.
    /// </summary>
    Warning,

    /// <summary>
    /// A serious issue that prevents proper compilation.
    /// </summary>
    Error
}