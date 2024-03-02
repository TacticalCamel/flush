namespace Interpreter;

using Microsoft.Extensions.Logging;
using Serialization;

internal static partial class LoggerMessageDefinitions {
    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Failed to read binary file, bytecode is corrupted")]
    public static partial void BytecodeCorrupted(this ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Bytecode version is mismatched (current version is {current}, but version {target} is targeted)")]
    public static partial void BytecodeVersionMismatch(this ILogger logger, Version target, Version current);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Running script:\r\n{script}")]
    public static partial void ExecutingScript(this ILogger logger, Script script);
    
    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Loaded {count} types in {time}")]
    public static partial void ClassesLoaded(this ILogger logger, int count, TimeSpan time);
}