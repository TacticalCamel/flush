namespace CLI.Commands;

using System.CommandLine;
using System.Text;
using Interpreter;
using Interpreter.Serialization;
using Options;
using Compiler;

internal sealed class RunCommand : Command {
    public RunCommand() : base("run", "Run a source file or a compiled executable.") {
        AddArgument(BuildOptionsBinder.InputFile);
        AddOption(BuildOptionsBinder.MinimumLogLevel);
        AddOption(BuildOptionsBinder.DisplayOutput);
        
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

        // try to read the source file
        SourceFile? sourceFile = SourceFile.TryRead(cliLogger, buildOptions.InputFile);

        // no valid source file, exit application
        if (sourceFile is null) {
            return;
        }

        switch (sourceFile.Extension) {
            // input is a source file, compile it and run the program
            case SourceFile.SOURCE_FILE_EXTENSION: {
                // create a logger for the compiler
                ILogger compilerLogger = factory.CreateLogger("Compiler");

                // convert from bytes to string
                string code = Encoding.UTF8.GetString(sourceFile.Contents);

                // create a new service
                CompilerService compilerService = new(compilerOptions, compilerLogger);

                // attempt to compile the program
                Script? script = compilerService.Compile(code);

                // compilation failed, exit
                if (script is null) {
                    break;
                }

                // display results
                if (buildOptions.DisplayOutput) {
                    script.WriteStringContents(Console.Out);
                }

                // create a new executor
                ScriptExecutor executor = new(script);

                // run the program
                executor.Run();
                
                break;
            }

            // input is a compiled file, run the program
            case SourceFile.COMPILED_FILE_EXTENSION: {
                // deserialize the script
                Script? script = BinarySerializer.BytesToScript(sourceFile.Contents, cliLogger);

                // corrupted file, exit
                if (script is null) {
                    break;
                }

                // create a new executor
                ScriptExecutor executor = new(script);

                // run the program
                executor.Run();

                break;
            }

            // invalid extension, log error message and exit
            default:
                cliLogger.TargetExtensionInvalid(SourceFile.SOURCE_FILE_EXTENSION, SourceFile.COMPILED_FILE_EXTENSION);
                break;
        }
    }
}