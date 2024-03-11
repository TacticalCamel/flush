namespace Compiler;

using System.ComponentModel.DataAnnotations;

public sealed class CompilerOptions {
    [Display(Name = "warnings-as-errors", ShortName = "werr", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; }

    [Display(Name = "no-warn", ShortName = "nw", Description = "Suppress one or more compiler warnings.")]
    public uint[] IgnoredWarningIds { get; init; } = [];
}