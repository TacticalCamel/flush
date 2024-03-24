namespace CLI;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Compiler;
using Interpreter;
using Interpreter.Serialization;
using Options;

internal static class Program {
    private static void Main(string[] args) {
        // parse the arguments of the program
        ParseArguments(args, out string[] targets, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions);

        // if requested, display help and exit
        if (interfaceOptions.DisplayHelp) {
            DisplayHelp();
            return;
        }

        // create console loggers
        CreateLoggers(interfaceOptions, out ILogger interfaceLogger, out ILogger compilerLogger, out ILogger interpreterLogger);

        // log application start
        interfaceLogger.ApplicationStarted(args);

        // try to read the source file
        TryReadSource(interfaceLogger, targets, out SourceFile? sourceFile);
        
        // no valid source file, exit application
        if (sourceFile is null) return;

        // switch based on the extension of the source file
        switch (sourceFile.Extension) {
            case SourceFile.FILE_SOURCE_EXTENSION: {
                // convert from bytes to string
                string code = Encoding.UTF8.GetString(sourceFile.Contents);

                // new CompilerService to compile code
                CompilerService compilerService = new(compilerLogger, compilerOptions);

                // attempt to compile the program
                Script? script = compilerService.Compile(code);

                // compilation failed, exit
                if (script is null) break;

                // display results
                if (interfaceOptions.DisplayResults) {
                    Console.WriteLine(script);
                }

                // run program
                if (interfaceOptions.ExecuteOnly) {
                    ScriptExecutor scriptExecutor = new(script);
                    scriptExecutor.Run();
                }
                // attempt to write the results to a file
                else {
                    TryWriteResult(interfaceLogger, interfaceOptions, script, sourceFile);
                }

                break;
            }

            case SourceFile.FILE_BINARY_EXTENSION: {
                // read from bytes
                Script? script = BinarySerializer.BytesToScript(sourceFile.Contents, interpreterLogger);

                // deserialization failed, exit
                if (script is null) break;

                // display deserialized binary file
                if (interfaceOptions.DisplayResults) {
                    Console.WriteLine(script);
                }

                // run program
                ScriptExecutor scriptExecutor = new(script);
                scriptExecutor.Run();

                break;
            }

            default: {
                // invalid extension, log error message and exit
                interfaceLogger.TargetExtensionInvalid(SourceFile.FILE_SOURCE_EXTENSION, SourceFile.FILE_BINARY_EXTENSION);
                break;
            }
        }
    }

    private static void ParseArguments(string[] args, out string[] targets, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions) {
        // create a factory for a temporary logger
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Debug)
            .AddConsole()
        );

        // create a temporary logger
        ILogger logger = factory.CreateLogger("Flags");

        // special dictionary key to use for targets
        // it will not be used by other flags, since none of them can have a name with 0 length
        string targetKey = string.Empty;

        // helper class to parse arguments
        OptionParser optionParser = new(logger, args, targetKey);

        // get target file paths
        targets = optionParser.GetAndRemoveOption(targetKey) ?? [];

        // get option values for 2 different option class
        interfaceOptions = optionParser.ParseFor<InterfaceOptions>();
        compilerOptions = optionParser.ParseFor<CompilerOptions>();

        // the remaining options were not recognized
        foreach (string option in optionParser.RemainingKeys) {
            logger.UnknownFlag(option);
        }
    }

    private static void DisplayHelp() {
        // get name and description pairs for the interface and the compiler
        Dictionary<string, string> interfaceHelpOptions = GetHelpOptionsFor<InterfaceOptions>();
        Dictionary<string, string> compilerHelpOptions = GetHelpOptionsFor<CompilerOptions>();

        // calculate the maximum length of the options
        // will need this pad the names with spaces, so the descriptions are aligned
        int padLength = Math.Max(interfaceHelpOptions.Max(x => x.Key.Length), compilerHelpOptions.Max(x => x.Key.Length)) + 4;

        // display the gathered information
        DisplayOptions(interfaceHelpOptions, "Interface options");
        DisplayOptions(compilerHelpOptions, "Compiler options");

        return;

        Dictionary<string, string> GetHelpOptionsFor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() {
            return typeof(T)
                .GetProperties()
                .Select(x => x.GetCustomAttribute<DisplayAttribute>())
                .Where(x => x is not null)
                .ToDictionary(GetKeyString!, GetValueString!);

            string GetKeyString(DisplayAttribute attribute) {
                string shortName = attribute.ShortName is null ? string.Empty : OptionParser.PREFIX_SHORT + attribute.ShortName;
                string name = attribute.Name is null ? string.Empty : OptionParser.PREFIX_LONG + attribute.Name;
                string separator = attribute.ShortName is not null && attribute.Name is not null ? ", " : string.Empty;
                
                return $"{name}{separator}{shortName}";
            }

            string GetValueString(DisplayAttribute attribute) {
                return attribute.Description ?? string.Empty;
            }
        }

        void DisplayOptions(Dictionary<string, string> helpOptions, string categoryName) {
            Console.WriteLine($"{categoryName}:");

            foreach (KeyValuePair<string, string> option in helpOptions) {
                Console.WriteLine($"    {option.Key.PadRight(padLength)}{option.Value}");
            }

            Console.WriteLine();
        }
    }

    private static void CreateLoggers(InterfaceOptions options, out ILogger interfaceLogger, out ILogger compilerLogger, out ILogger interpreterLogger) {
        // create a factory with the provided minimum log level
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(options.MinimumLogLevel)
            .AddConsole()
        );

        // create a logger for every module
        interfaceLogger = factory.CreateLogger("Interface");
        compilerLogger = factory.CreateLogger("Compiler");
        interpreterLogger = factory.CreateLogger("Interpreter");
    }

    private static void TryReadSource(ILogger logger, string[] targets, out SourceFile? sourceFile) {
        // null as default value
        sourceFile = null;

        // not providing a target is not accepted
        if (targets.Length == 0) {
            logger.NoTarget();
            return;
        }

        // providing multiple targets is also not accepted
        if (targets.Length > 1) {
            logger.MultipleTargets();
            return;
        }

        // exactly 1 target
        // try block to catch any IO exception
        try {
            FileInfo file = new(targets[0]);

            // file does not exists
            if (!file.Exists) {
                logger.TargetDoesNotExist(file.FullName);
            }
            // path is not a file, but a directory
            else if ((file.Attributes & FileAttributes.Directory) != 0) {
                logger.TargetCannotBeDirectory(file.FullName);
            }
            // no basic errors, read source file
            else {
                byte[] contents = File.ReadAllBytes(file.FullName);
                sourceFile = new SourceFile(file, contents);
            }
        }
        // catch any other error
        catch (Exception e) {
            logger.FileWriteFailed(e.Message);
        }
    }

    private static void TryWriteResult(ILogger logger, InterfaceOptions interfaceOptions, Script script, SourceFile sourceFile) {
        // if no custom output path is provided, put the file to the same directory as the input
        string outputPath = interfaceOptions.OutputPath ?? Path.ChangeExtension(sourceFile.FullPath, interfaceOptions.CompileToPlainText ? SourceFile.FILE_TEXT_EXTENSION : SourceFile.FILE_BINARY_EXTENSION);

        // attempt to write to output file
        try {
            if (interfaceOptions.CompileToPlainText) {
                string text = script.ToString();
                File.WriteAllText(outputPath, text);
            }
            else {
                byte[] bytes = BinarySerializer.ScriptToBytes(script);
                File.WriteAllBytes(outputPath, bytes);
            }

            logger.FileWriteSuccess(outputPath);
        }
        // catch any IO error
        catch (Exception e) {
            logger.FileWriteFailed(e.Message);
        }
    }
}