namespace Interpreter;

using Microsoft.Extensions.Logging;

internal static partial class LoggerMessageDefinitions {
    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Failed to read binary file, bytecode is corrupted")]
    public static partial void BytecodeCorrupted(this ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Bytecode version is mismatched (current version is {current}, but version {target} is targeted)")]
    public static partial void BytecodeVersionMismatch(this ILogger logger, string target, string current);
}