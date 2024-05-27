namespace CLI;

using System.Text;
using System.CommandLine;
using IO;
using Compiler;
using Interpreter.Serialization;

/// <summary>
/// The starting point of the application, interacts with the public interfaces of the compiler and the interpreter.
/// </summary>
internal static class Program {
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The exit code of the application.</returns>
    private static int Main(string[] args) {
        Command build = new("build", "Compile a source file into an executable.") {
            CommandLineOptions.MinimumLogLevel,
            CommandLineOptions.InputFile,
            CommandLineOptions.DisplayResults,
            CommandLineOptions.OutputPath,
            CommandLineOptions.PlainText,
            CommandLineOptions.WarningsAsErrors,
            CommandLineOptions.SuppressIssues,
        };

        build.SetHandler(
            HandleBuild,
            CommandLineOptions.MinimumLogLevel,
            CommandLineOptions.InputFile,
            CommandLineOptions.DisplayResults,
            CommandLineOptions.OutputPath,
            CommandLineOptions.PlainText
        );

        Command run = new("run", "Run a source file or a compiled executable.") {
            CommandLineOptions.InputFile
        };

        run.SetHandler(
            HandleRun,
            CommandLineOptions.MinimumLogLevel,
            CommandLineOptions.InputFile,
            CommandLineOptions.DisplayResults
        );

        RootCommand root = new("Root command.") {
            build,
            run
        };

        return root.Invoke(args);
    }

    private static void HandleBuild(LogLevel minimumLogLevel, FileInfo file, bool displayResults, string? outputPath, bool compilePlainText) {
        // create a factory with the provided minimum log level
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(minimumLogLevel)
            .AddConsole()
        );

        // create a logger for the CLI
        ILogger cliLogger = factory.CreateLogger("CLI");

        // create a logger for the compiler
        ILogger compilerLogger = factory.CreateLogger("Compiler");

        // try to read the source file
        SourceFile? sourceFile = FileOperations.TryRead(cliLogger, file);

        // no valid source file, exit application
        if (sourceFile is null) {
            return;
        }

        // invalid extension, log error message and exit
        if (sourceFile.Extension != SourceFile.SOURCE_FILE_EXTENSION) {
            cliLogger.TargetExtensionInvalid(SourceFile.SOURCE_FILE_EXTENSION, SourceFile.COMPILED_FILE_EXTENSION);
            return;
        }

        // convert from bytes to string
        string code = Encoding.UTF8.GetString(sourceFile.Contents);

        // create a new service
        // TODO supply options from the CLI
        CompilerService compilerService = new(new CompilerOptions(), compilerLogger);

        // attempt to compile the program
        Script? script = compilerService.Compile(code);

        // compilation failed, exit
        if (script is null) {
            return;
        }

        // display results
        if (displayResults) {
            script.WriteStringContents(Console.Out);
        }

        // attempt to write the results to a file
        FileOperations.TryWrite(cliLogger, script, sourceFile, outputPath, compilePlainText);
    }

    private static void HandleRun(LogLevel minimumLogLevel, FileInfo file, bool displayResults) {
        // TODO implement   
    }
}