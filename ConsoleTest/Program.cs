namespace ConsoleTest;

using Compiler;
using Microsoft.Extensions.Logging;

internal static class Program {
    private static void Main(string[] args) {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole()
        );

        ILogger interfaceLogger = factory.CreateLogger("Program");
        ILogger compilerLogger = factory.CreateLogger("Compiler");

        string? code = GetSourceCode(args, interfaceLogger);
        if (code is null) return;

        CompilerService compilerService = new(compilerLogger);
        CompilerOptions options = CompilerOptions.FromConsoleArgs(args);
        
        compilerService.Build(code, options);
    }

    private static string? GetSourceCode(string[] args, ILogger logger) {
        string[] targets = args.Where(arg => !arg.StartsWith('-')).ToArray();

        switch (targets.Length) {
            case > 1:
                logger.MultipleTargets();
                return null;
            case 0:
                logger.NoTarget();
                return null;
            default:
                string path = args[0];

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