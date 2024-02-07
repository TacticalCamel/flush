namespace Compiler;

using System.ComponentModel.DataAnnotations;

public sealed class CompilerOptions {
    [Display(Name = "static", ShortName = "s", Description = "Include referenced code into the compiled program.")]
    public bool IsStatic { get; init; }

    [Display(Name = "warnings-as-errors", ShortName = "wae", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; }

    [Display(Name = "plain-text", ShortName = "pt", Description = "Compile to a plain text format instead of bytecode.")]
    public bool CompileToPlainText { get; init; }

    [Display(Name = "no-warn", ShortName = "nw", Description = "Suppress one or more compiler warnings.")]
    public uint[] IgnoredWarningIds { get; init; } = [];
}