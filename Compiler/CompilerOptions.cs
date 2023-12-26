namespace Compiler;

using Analysis;
using System.ComponentModel.DataAnnotations;

internal sealed class CompilerOptions {
    [Display(Name = "help", ShortName = "h", Description = "Display help options.")]
    public bool DisplayHelp { get; init; } = false;

    [Display(Name = "static", ShortName = "s", Description = "Include referenced code into the compiled program.")]
    public bool IsStatic { get; init; } = false;

    [Display(Name = "warnings-as-errors", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; } = false;

    [Display(Name = "ignore-warnings", Description = "Ignore all listed non-fatal warnings. This may cause a faulty program to be compiled anyway.")]
    public int[] IgnoredWarningIds { get; init; } = [];

    [Display(Name = "no-meta", Description = "Do not include metadata in the compiled program.")]
    public bool IncludeMetaData { get; init; } = true;

    [Display(Name = "plain-text", Description = "Compile to a plain text representation instead of bytecode. The output file is not executable, but human-readable for debug purposes.")]
    public bool CompileToPlainText { get; init; } = false;

    [Display(Name = "output", ShortName = "o", Description = "Output the compiled program to the given directory on the local machine.")]
    public string? OutputDirectory { get; init; } = null;

    [Display(Name = "file", Description = "The path of the input file to compile.")]
    public string? SourceCode { get; init; } = null;
    
    [Display(Name = "code", Description = "The source code to compile.")]
    public string? SourcePath { get; init; } = null;

    private CompilerOptions() {
        
    }
    
    public static CompilerOptions? CreateFromArgs(string[] args, List<Warning> warnings) {
        return null;
    }
}
