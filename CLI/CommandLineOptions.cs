namespace CLI;

using System.CommandLine;

internal static class CommandLineOptions {
    public static Argument<FileInfo> InputFile { get; } = new("input-file", "The path of the input file.");
    
    public static Option<bool> WarningsAsErrors { get; } = new(["--warnings-as-errors", "-werr"], "Treat warnings as if they were errors.");
    
    public static Option<bool> PlainText { get; } = new(["--plain-text", "-pt"], "Output compilation results to plain text instead of bytecode.");
    
    public static Option<uint[]> SuppressIssues { get; } = new(["--no-warn", "-nw"], "Suppress one or more compiler issues. Errors can not be suppressed.") {
        AllowMultipleArgumentsPerToken = true
    };
    
    public static Option<LogLevel> MinimumLogLevel { get; } = new(["--verbose", "-v"], "Set verbosity level. Allowed values are t(race), d(ebug), i(nformation), w(arning), e(rror), c(ritical), n(one).");
    
    public static Option<string> OutputPath { get; } = new(["--output", "-o"], "Set output path.");
    
    public static Option<bool> DisplayResults { get; } = new(["--display", "-d"], "Display the compilation results.");

    static CommandLineOptions() {
        MinimumLogLevel.SetDefaultValue(LogLevel.Information);
    }
}