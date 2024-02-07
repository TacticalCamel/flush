namespace ConsoleInterface;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Compiler;
using Interpreter;
using Interpreter.Bytecode;
using Options;

internal static class Program {
    private const string FILE_SOURCE_EXTENSION = ".sra";
    private const string FILE_BINARY_EXTENSION = ".bin";
    private const string FILE_TEXT_EXTENSION = ".txt";

    private static void Main(string[] args) {
        // parse the arguments of the program
        ParseArguments(args, out string[] targets, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions);

        // if requested, display help and exit
        if (interfaceOptions.DisplayHelp) {
            DisplayHelp();
            return;
        }

        // create console loggers
        CreateLoggers(interfaceOptions, out ILogger interfaceLogger, out ILogger compilerLogger);

        // log application start
        interfaceLogger.ApplicationStart(args);
        
        // try to read the source file
        ReadSourceFile(interfaceLogger, targets, out SourceFile? sourceFile);
        
        // no valid source file, exit application
        if (sourceFile is null) return;
        
        // switch based on the extension of the source file
        switch (sourceFile.Extension) {
            case FILE_SOURCE_EXTENSION:
                // convert from bytes to string
                string code = Encoding.UTF8.GetString(sourceFile.Contents);
                
                // new CompilerService to compile code
                CompilerService compilerService = new(compilerLogger, compilerOptions);
                
                // attempt to compile the program
                Script? script = compilerService.Compile(code);
                
                // compilation failed, exit
                if (script is null) break;
                
                if (interfaceOptions.ExecuteOnly) {
                    // run program
                    interfaceLogger.RunScript();
                    ScriptExecutor.Run(script);
                }
                else {
                    // attempt to write the results to a file
                    WriteResults(interfaceLogger, interfaceOptions, compilerOptions, script, sourceFile);
                }
                
                break;
            
            case FILE_BINARY_EXTENSION:
                script = Script.CreateFromBytes(sourceFile.Contents, interfaceLogger);

                if (script is not null) {
                    ScriptExecutor.Run(script);
                }
                
                break;
            
            default:
                // invalid extension, log error message and exit
                interfaceLogger.TargetExtensionInvalid(FILE_SOURCE_EXTENSION, FILE_BINARY_EXTENSION);
                break;
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
        OptionsParser optionsParser = new(logger, args, targetKey);

        // get target file paths
        targets = optionsParser.GetAndRemoveOption(targetKey) ?? Array.Empty<string>();

        // get option values for 2 different option class
        interfaceOptions = optionsParser.ParseFor<InterfaceOptions>();
        compilerOptions = optionsParser.ParseFor<CompilerOptions>();

        // the remaining options were not recognized
        foreach (string option in optionsParser.GetRemainingOptionNames()) {
            logger.UnknownFlag(option);
        }
    }

    private static void DisplayHelp() {
        // get name and description pairs for the interface and the compiler
        Dictionary<string, string> interfaceHelpOptions = GetHelpOptionsFor<InterfaceOptions>();
        Dictionary<string, string> compilerHelpOptions = GetHelpOptionsFor<CompilerOptions>();

        // calculate the maximum length of the options
        // will need this pad the names with spaces, so the descriptions are aligned
        int length = Math.Max(interfaceHelpOptions.Max(x => x.Key.Length), compilerHelpOptions.Max(x => x.Key.Length)) + 4;

        // display the gathered information
        DisplayOptions(interfaceHelpOptions, length, "Interface options");
        DisplayOptions(compilerHelpOptions, length, "Compiler options");

        return;

        Dictionary<string, string> GetHelpOptionsFor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() {
            return typeof(T)
                .GetProperties()
                .Select(x => x.GetCustomAttribute<DisplayAttribute>())
                .Where(x => x != null)
                .ToDictionary(x => $"{(x!.Name is null ? string.Empty : $"{OptionsParser.PREFIX_LONG}{x.Name}")}{(x.ShortName is null ? string.Empty : $", {OptionsParser.PREFIX_SHORT}{x.ShortName}")}", x => x!.Description ?? string.Empty);
        }

        void DisplayOptions(Dictionary<string, string> helpOptions, int padLength, string categoryName) {
            Console.WriteLine($"{categoryName}:");

            foreach (KeyValuePair<string, string> option in helpOptions) {
                Console.WriteLine($"    {option.Key.PadRight(padLength)}{option.Value}");
            }

            Console.WriteLine();
        }
    }
    
    private static void CreateLoggers(InterfaceOptions options, out ILogger interfaceLogger, out ILogger compilerLogger) {
        // create a factory with the provided minimum log level
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(options.MinimumLogLevel)
            .AddConsole()
        );

        // create 2 loggers for use by this console interface and the compiler itself
        interfaceLogger = factory.CreateLogger("Interface");
        compilerLogger = factory.CreateLogger("Compiler");
    }

    private static void ReadSourceFile(ILogger logger, string[] targets, out SourceFile? sourceFile) {
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
                logger.TargetMustBeFile(file.FullName);
            }
            // no basic errors, read source file
            else {
                byte[] contents = File.ReadAllBytes(file.FullName);
                sourceFile = new SourceFile(file, contents);
            }
        }
        // catch any other error
        catch (Exception e) {
            logger.FileError(e.Message);
        }
    }

    private static void WriteResults(ILogger logger, InterfaceOptions interfaceOptions, CompilerOptions compilerOptions, Script script, SourceFile sourceFile) {
        // if no custom output path is provided, put the file to the same directory as the input
        string outputPath = interfaceOptions.OutputPath ?? Path.ChangeExtension(sourceFile.FullPath, compilerOptions.CompileToPlainText ? FILE_TEXT_EXTENSION : FILE_BINARY_EXTENSION);

        // attempt to write to output file
        try {
            if (compilerOptions.CompileToPlainText) {
                string text = script.ToString();
                File.WriteAllText(outputPath, text);
            }
            else {
                byte[] bytes = script.ToBytes();
                File.WriteAllBytes(outputPath, bytes);
            }
            logger.OutputToPath(outputPath);
        }
        // catch any IO error
        catch (Exception e) {
            logger.FileError(e.Message);
        }
    }
}