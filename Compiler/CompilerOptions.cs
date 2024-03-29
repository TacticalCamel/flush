namespace Compiler;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a configuration of the compiler.
/// </summary>
public sealed class CompilerOptions {
    [Display(Name = "warnings-as-errors", ShortName = "werr", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; }

    [Display(Name = "no-warn", ShortName = "nw", Description = "Suppress one or more compiler issues. Errors can not be supressed.")]
    public uint[] IgnoredIssues { get; init; } = [];
}