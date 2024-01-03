namespace Compiler;

using Microsoft.Extensions.Logging;

internal static partial class LoggerMessageDefinitions {
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{message}")]
    public static partial void BuildHint(this ILogger logger, string message);
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "{message}")]
    public static partial void BuildWarning(this ILogger logger, string message);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "{message}")]
    public static partial void BuildError(this ILogger logger, string message);
    
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Build succeeded. Errors: {errorCount}.  Warnings: {warningCount}.")]
    public static partial void BuildResultSuccess(this ILogger logger, int errorCount, int warningCount);
    
    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Build failed. Errors: {errorCount}.  Warnings: {warningCount}.")]
    public static partial void BuildResultFail(this ILogger logger, int errorCount, int warningCount);
    
    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "Unexpected build error")]
    public static partial void UnexpectedBuildError(this ILogger logger, Exception e);
    
    [LoggerMessage(EventId = 100, Level = LogLevel.Debug, Message = "{obj}")]
    public static partial void Debug(this ILogger logger, object obj);
}