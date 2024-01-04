namespace ConsoleInterface;

using Compiler;
using Options;

internal static class Program {
    private const string FILE_INPUT_EXTENSION = ".sra";
    private const string FILE_OUTPUT_EXTENSION = ".bin";

    private static void Main(string[] args) {
        // parse the arguments of the program
        ParseArguments(args, out string[] inputPaths, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions);

        // create a pair of loggers
        CreateLoggers(interfaceOptions, out ILogger interfaceLogger, out ILogger compilerLogger);

        // log application start
        interfaceLogger.ApplicationStart(args);

        // if requested, display help and exit
        if (interfaceOptions.DisplayHelp) {
            DisplayHelp();
            return;
        }

        // try to get the input source code
        GetInputCode(interfaceLogger, inputPaths, out string? inputPath, out string? code);

        // no valid source file, exit application
        if (inputPath is null || code is null) return;

        // new CompilerService to compile code
        CompilerService compilerService = new(compilerLogger, compilerOptions);

        // attempt to compile the program
        compilerService.Compile(code, out byte[]? results);

        // failed compilation, exit application
        if (results is null) return;

        if (interfaceOptions.ExecuteOnly) {
            // TODO not yet implemented
            interfaceLogger.FeatureNotImplemented("Script execution");
        }
        else {
            // attempt to write the results to a file
            WriteResults(interfaceLogger, interfaceOptions, results, inputPath);
        }
    }

    private static void ParseArguments(string[] args, out string[] inputPaths, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions) {
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
        inputPaths = optionsParser.GetAndRemoveOption(targetKey) ?? Array.Empty<string>();

        // get option values for 2 different option class
        interfaceOptions = optionsParser.ParseFor<InterfaceOptions>();
        compilerOptions = optionsParser.ParseFor<CompilerOptions>();

        // the remaining options were not recognized
        foreach (string option in optionsParser.GetRemainingOptionNames()) {
            logger.UnknownFlag(option);
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

        Dictionary<string, string> GetHelpOptionsFor<T>() {
            return typeof(T)
                .GetProperties()
                .Select(x => x.GetCustomAttribute<DisplayAttribute>())
                .Where(x => x != null)
                .ToDictionary(x => $"{(x!.Name is null ? string.Empty : $"--{x.Name}")}{(x.ShortName is null ? string.Empty : $", -{x.ShortName}")}", x => x!.Description ?? string.Empty);
        }

        void DisplayOptions(Dictionary<string, string> helpOptions, int padLength, string categoryName) {
            Console.WriteLine($"{categoryName}:");

            foreach (KeyValuePair<string, string> option in helpOptions) {
                Console.WriteLine($"    {option.Key.PadRight(padLength)}{option.Value}");
            }

            Console.WriteLine();
        }
    }

    private static void GetInputCode(ILogger logger, string[] targets, out string? inputPath, out string? code) {
        // null for default values
        inputPath = null;
        code = null;

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
        // try block to catch any IO exception, since the input string is from the user and not to be trusted in any way
        try {
            FileInfo file = new(targets[0]);

            // file does not exists
            if (!file.Exists) {
                logger.TargetDoesNotExist(targets[0]);
            }
            // path is not a file, but a directory
            else if ((file.Attributes & FileAttributes.Directory) != 0) {
                logger.TargetMustBeFile(targets[0]);
            }
            // file extension is not valid
            else if (!file.Extension.Equals(FILE_INPUT_EXTENSION, StringComparison.CurrentCultureIgnoreCase)) {
                logger.TargetExtensionInvalid(FILE_INPUT_EXTENSION);
            }
            // no basic errors, try to read file contents
            else {
                code = File.ReadAllText(targets[0]);
                inputPath = Path.GetFullPath(targets[0]);
            }
        }
        // catch any other error, like lack of permission to read the file
        catch (Exception e) {
            logger.FileError(e.Message);
        }
    }

    private static void WriteResults(ILogger logger, InterfaceOptions options, byte[] results, string inputPath) {
        // if no custom output path is provided, put the file to the same directory as the input
        string outputPath = options.OutputPath ?? Path.ChangeExtension(inputPath, FILE_OUTPUT_EXTENSION);

        // attempt to write into the file
        try {
            File.WriteAllBytes(outputPath, results);
            logger.OutputToPath(outputPath);
        }
        // catch any IO error
        catch (Exception e) {
            logger.FileError(e.Message);
        }
    }
}