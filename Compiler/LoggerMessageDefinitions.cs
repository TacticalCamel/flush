namespace Compiler;

using Microsoft.Extensions.Logging;

internal static partial class LoggerMessageDefinitions {
    [LoggerMessage(Level = LogLevel.Information, Message = "Successful build")]
    public static partial void BuildSuccessful(this ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Build cancelled after error")]
    public static partial void BuildFailed(this ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Critical, Message = "Unexpected build error")]
    public static partial void UnexpectedBuildError(this ILogger logger, Exception e);
    
    [LoggerMessage(Level = LogLevel.Debug, Message = "{obj}")]
    public static partial void Debug(this ILogger logger, object obj);
}