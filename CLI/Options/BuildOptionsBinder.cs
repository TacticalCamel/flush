namespace CLI.Options;

using System.CommandLine;
using System.CommandLine.Binding;

internal sealed class BuildOptionsBinder : BinderBase<BuildOptions> {
    public static Argument<FileInfo> InputFile { get; } = new(
        name: "input-file",
        description: "The path of the input file."
    );

    public static Option<bool> OutputPlainText { get; } = new(
        aliases: ["--plain-text", "-pt"],
        description: "Output compilation results to plain text instead of bytecode."
    );

    public static Option<LogLevel> MinimumLogLevel { get; } = new(
        aliases: ["--verbose", "-v"],
        description: "Set verbosity level. Allowed values are t(race), d(ebug), i(nformation), w(arning), e(rror), c(ritical), n(one).",
        getDefaultValue: () => LogLevel.Information
    );

    public static Option<string> OutputPath { get; } = new(
        aliases: ["--output", "-o"],
        description: "Set output path."
    );

    public static Option<bool> DisplayOutput { get; } = new(
        aliases: ["--display", "-d"],
        description: "Display the compilation results."
    );
    
    public static Option<bool> IgnoreFileExtension { get; } = new(
        aliases: ["--ignore-extension", "-ie"],
        description: "Ignore file extension requirements when reading an input file."
    );

    protected override BuildOptions GetBoundValue(BindingContext bindingContext) {
        return new BuildOptions {
            InputFile = bindingContext.ParseResult.GetValueForArgument(InputFile),
            OutputPlainText = bindingContext.ParseResult.GetValueForOption(OutputPlainText),
            MinimumLogLevel = bindingContext.ParseResult.GetValueForOption(MinimumLogLevel),
            OutputPath = bindingContext.ParseResult.GetValueForOption(OutputPath),
            DisplayOutput = bindingContext.ParseResult.GetValueForOption(DisplayOutput),
            IgnoreFileExtension = bindingContext.ParseResult.GetValueForOption(IgnoreFileExtension)
        };
    }
}