namespace ConsoleInterface.Logging;

using Microsoft.Extensions.Logging;

internal static partial class LoggerMessageDefinitions {
    #region General

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Started application with arguments: [{args}]")]
    public static partial void ApplicationStart(this ILogger logger, string[] args);

    #endregion


    #region Flags

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Property of type {propertyType} in class {objectType} can not be parsed")]
    public static partial void PropertyNotParseable(this ILogger logger, Type propertyType, Type objectType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Flag \"{flag}\" is used multiple times, ignoring duplicate values")]
    public static partial void DuplicateFlag(this ILogger logger, string flag);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Unknown flag \"{flag}\"")]
    public static partial void UnknownFlag(this ILogger logger, string flag);
    
    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Value {values} for flag \"{flag}\" is invalid.")]
    public static partial void InvalidFlagValue(this ILogger logger, string flag, string[] values);

    #endregion


    #region Target

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Multiple targets are not supported")]
    public static partial void MultipleTargets(this ILogger logger);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Target at \"{path}\" does not exist")]
    public static partial void TargetDoesNotExist(this ILogger logger, string path);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Target is invalid")]
    public static partial void TargetInvalid(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "No target was specified")]
    public static partial void NoTarget(this ILogger logger);

    #endregion
}