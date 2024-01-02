namespace Compiler;

using System.ComponentModel.DataAnnotations;

public sealed class CompilerOptions {
    [Display(Name = "static", ShortName = "s", Description = "Include referenced code into the compiled program.")]
    public bool IsStatic { get; init; }

    [Display(Name = "warnings-as-errors", ShortName = "wae", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; }

    [Display(Name = "no-meta", ShortName = "nm", Description = "Do not include metadata in the compiled program.")]
    public bool ExcludeMetaData { get; init; }

    [Display(Name = "plain-text", ShortName = "pt", Description = "Compile to a plain text representation instead of bytecode.")]
    public bool CompileToPlainText { get; init; }
    
    [Display(Name = "execute-only", ShortName = "exe", Description = "Do not create an output file, only compile and execute in memory instead.")]
    public bool ExecuteOnly { get; init; }
}
