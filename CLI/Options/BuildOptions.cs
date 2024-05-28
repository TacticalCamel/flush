namespace CLI.Options;

internal sealed record BuildOptions {
    public required LogLevel MinimumLogLevel { get; init; }
    public required FileInfo InputFile { get; init; }
    public required string? OutputPath { get; init; }
    public required bool OutputPlainText { get; init; }
    public required bool DisplayOutput { get; init; }
    public required bool IgnoreFileExtension { get; init; }
}