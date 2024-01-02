namespace ConsoleInterface;

using Compiler;
using Logging;
using Options;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

internal static class Program {
    private static void Main(string[] args) {
        ParseArguments(args, out string[]? targets, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions);
        CreateLoggers(interfaceOptions, out ILogger interfaceLogger, out ILogger compilerLogger);

        if (interfaceOptions.DisplayHelp) {
            DisplayHelp();
            return;
        }

        interfaceLogger.ApplicationStart(args);
        
        string? code = GetSourceCode(targets, interfaceLogger);
        if (code is null) return;

        CompilerService compilerService = new(compilerLogger);

        compilerService.Build(code, compilerOptions);
    }

    private static void ParseArguments(string[] args, out string[]? targets, out InterfaceOptions interfaceOptions, out CompilerOptions compilerOptions) {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Debug)
            .AddConsole()
        );

        ILogger logger = factory.CreateLogger("Flags");

        string targetKey = string.Empty;
        OptionsParser optionsParser = new(logger, args, targetKey);

        interfaceOptions = optionsParser.ParseFor<InterfaceOptions>();
        compilerOptions = optionsParser.ParseFor<CompilerOptions>();

        IEnumerable<string> unknownOptions = optionsParser.GetRemainingOptionNames();

        targets = optionsParser.GetAndRemoveOption(targetKey);
        
        foreach (string option in unknownOptions) {
            logger.UnknownFlag(option);
        }
    }

    private static void CreateLoggers(InterfaceOptions options, out ILogger interfaceLogger, out ILogger compilerLogger) {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(options.MinimumLogLevel)
            .AddConsole()
        );

        interfaceLogger = factory.CreateLogger("Interface");
        compilerLogger = factory.CreateLogger("Compiler");
    }

    private static void DisplayHelp() {
        Dictionary<string, string> interfaceOptions = GetHelpOptionsFor<InterfaceOptions>();
        Dictionary<string, string> compilerOptions = GetHelpOptionsFor<CompilerOptions>();

        int length = Math.Max(interfaceOptions.Max(x => x.Key.Length), compilerOptions.Max(x => x.Key.Length)) + 4;

        DisplayOptions(interfaceOptions, length, "Interface options");
        DisplayOptions(compilerOptions, length, "Compiler options");

        return;

        Dictionary<string, string> GetHelpOptionsFor<T>() {
            return typeof(T)
                .GetProperties()
                .Select(x => x.GetCustomAttribute<DisplayAttribute>())
                .Where(x => x != null)
                .ToDictionary(x => $"{(x!.Name is null ? string.Empty : $"--{x.Name}")}{(x.ShortName is null ? string.Empty : $", -{x.ShortName}")}", x => x!.Description ?? string.Empty);
        }

        void DisplayOptions(Dictionary<string, string> options, int padLength, string categoryName) {
            Console.WriteLine($"{categoryName}:");

            foreach (KeyValuePair<string, string> option in options) {
                Console.WriteLine($"    {option.Key.PadRight(padLength)}{option.Value}");
            }

            Console.WriteLine();
        }
    }

    private static string? GetSourceCode(string[]? targets, ILogger logger) {
        if (targets is null) {
            logger.NoTarget();
            return null;
        }

        switch (targets.Length) {
            case > 1:
                logger.MultipleTargets();
                return null;
            case 0:
                logger.NoTarget();
                return null;
            default:
                string path = targets[0];

                try {
                    return File.ReadAllText(path);
                }
                catch (FileNotFoundException) {
                    logger.TargetDoesNotExist(path);
                }
                catch (Exception e) {
                    logger.TargetInvalid(e);
                }

                return null;
        }
    }
}