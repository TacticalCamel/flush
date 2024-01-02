namespace ConsoleInterface.Options;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

internal sealed class InterfaceOptions {
    [Display(Name = "help", ShortName = "h", Description = "Show help and exit.")]
    public bool DisplayHelp { get; init; } = false;

    [Display(Name = "verbose", ShortName = "v", Description = "Set verbosity level. Allowed values are d(ebug), i(nformation), w(arning), e(rror), c(ritical), n(one).")]
    public LogLevel MinimumLogLevel { get; init; } = LogLevel.Warning;

    [Display(Name = "output", ShortName = "o", Description = "Set output path.")]
    public string? OutputPath { get; init; } = null;

    [Display(Name = "execute-only", ShortName = "exe", Description = "Do not create an output file, only compile and execute in memory instead.")]
    public bool ExecuteOnly { get; init; } = false;
}