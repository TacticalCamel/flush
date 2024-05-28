namespace CLI.Options;

using System.CommandLine;
using System.CommandLine.Binding;
using Compiler;

internal sealed class CompilerOptionsBinder : BinderBase<CompilerOptions> {
    public static Option<bool> WarningsAsErrors { get; } = new(
        aliases: ["--warnings-as-errors", "-werr"],
        description: "Treat warnings as if they were errors."
    );

    public static Option<uint[]> SuppressIssues { get; } = new(
        aliases: ["--suppress", "-s"],
        description: "Suppress one or more compiler issues. Errors can not be suppressed.",
        getDefaultValue: () => []
    ) { AllowMultipleArgumentsPerToken = true };

    protected override CompilerOptions GetBoundValue(BindingContext bindingContext) {
        return new CompilerOptions {
            WarningsAsErrors = bindingContext.ParseResult.GetValueForOption(WarningsAsErrors),
            SuppressIssues = bindingContext.ParseResult.GetValueForOption(SuppressIssues)!
        };
    }
}