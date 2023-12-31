namespace ConsoleTest;

using Microsoft.Extensions.Logging;

internal static partial class LoggerMessageDefinitions {
    [LoggerMessage(Level = LogLevel.Warning, Message = "{name} is not implemented and will be ignored")]
    public static partial void FeatureNotImplemented(this ILogger logger, string name);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "No target was specified")]
    public static partial void NoTarget(this ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Multiple targets are not supported")]
    public static partial void MultipleTargets(this ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Target at \"{path}\" does not exist")]
    public static partial void TargetDoesNotExist(this ILogger logger, string path);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Target is invalid")]
    public static partial void TargetInvalid(this ILogger logger, Exception e);
}