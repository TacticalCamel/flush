namespace ConsoleInterface.Options;

internal sealed class InterfaceOptions {
    [Display(Name = "help", ShortName = "h", Description = "Show help and exit.")]
    public bool DisplayHelp { get; init; }

    [Display(Name = "verbose", ShortName = "v", Description = "Set verbosity level. Allowed values are d(ebug), i(nformation), w(arning), e(rror), c(ritical), n(one).")]
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;

    [Display(Name = "output", ShortName = "o", Description = "Set output path.")]
    public string? OutputPath { get; init; }
    
    [Display(Name = "execute", ShortName = "x", Description = "Do not create an output file, execute instead")]
    public bool ExecuteOnly { get; init; }
}