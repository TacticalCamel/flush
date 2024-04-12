namespace CLI.Options;

/// <summary>
/// Represents a configuration of the CLI.
/// </summary>
internal sealed class InterfaceOptions {
    [Display(Name = "help", ShortName = "h", Description = "Show help and exit.")]
    public bool DisplayHelp { get; init; }
    
    [Display(Name = "version", Description = "Show the version of the application and exit.")]
    public bool DisplayVersion { get; init; }

    [Display(Name = "display", ShortName = "d", Description = "Display results in plain text format.")]
    public bool DisplayResults { get; init; }

    [Display(Name = "verbose", ShortName = "v", Description = "Set verbosity level. Allowed values are t(race), d(ebug), i(nformation), w(arning), e(rror), c(ritical), n(one).")]
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;

    [Display(Name = "output", ShortName = "o", Description = "Set output path.")]
    public string? OutputPath { get; init; }

    [Display(Name = "execute", ShortName = "x", Description = "Do not create an output file, execute instead.")]
    public bool ExecuteOnly { get; init; }

    [Display(Name = "plain-text", ShortName = "pt", Description = "Compile to a plain text format instead of bytecode.")]
    public bool CompileToPlainText { get; init; }
}