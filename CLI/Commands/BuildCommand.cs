namespace CLI.Commands;

using System.CommandLine;
using System.Text;
using Interpreter.Serialization;
using Compiler;
using Options;

internal sealed class BuildCommand : Command {
    public BuildCommand() : base("build", "Compile a source file into an executable.") {
        AddArgument(BuildOptionsBinder.InputFile);
        AddOption(BuildOptionsBinder.MinimumLogLevel);
        AddOption(BuildOptionsBinder.DisplayOutput);
        AddOption(BuildOptionsBinder.OutputPath);
        AddOption(BuildOptionsBinder.OutputPlainText);
        AddOption(BuildOptionsBinder.IgnoreFileExtension);
        
        AddOption(CompilerOptionsBinder.WarningsAsErrors);
        AddOption(CompilerOptionsBinder.SuppressIssues);

        this.SetHandler(
            Handle,
            new BuildOptionsBinder(),
            new CompilerOptionsBinder()
        );
    }

    private static void Handle(BuildOptions buildOptions, CompilerOptions compilerOptions) {
        // create a factory with the provided minimum log level
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(buildOptions.MinimumLogLevel)
            .AddConsole()
        );

        // create a logger for the CLI
        ILogger cliLogger = factory.CreateLogger("CLI");

        // create a logger for the compiler
        ILogger compilerLogger = factory.CreateLogger("Compiler");

        // try to read the source file
        SourceFile? sourceFile = SourceFile.TryRead(cliLogger, buildOptions.InputFile);

        // no valid source file, exit application
        if (sourceFile is null) {
            return;
        }

        // invalid extension, log error message and exit
        if (!buildOptions.IgnoreFileExtension && sourceFile.Extension != SourceFile.SOURCE_FILE_EXTENSION) {
            cliLogger.TargetExtensionInvalid(SourceFile.SOURCE_FILE_EXTENSION, SourceFile.COMPILED_FILE_EXTENSION);
            return;
        }

        // convert from bytes to string
        string code = Encoding.UTF8.GetString(sourceFile.Contents);

        // create a new service
        CompilerService compilerService = new(compilerOptions, compilerLogger);

        // attempt to compile the program
        Script? script = compilerService.Compile(code);

        // compilation failed, exit
        if (script is null) {
            return;
        }

        // display results
        if (buildOptions.DisplayOutput) {
            script.WriteStringContents(Console.Out);
        }

        // attempt to write the results to a file
        SourceFile.TryWrite(cliLogger, script, sourceFile, buildOptions.OutputPath, buildOptions.OutputPlainText);
    }
}