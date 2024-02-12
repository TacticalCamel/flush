namespace ConsoleInterface;

internal static partial class LoggerMessageDefinitions {
    #region General

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Started application with arguments: [{args}]")]
    public static partial void ApplicationStarted(this ILogger logger, string[] args);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "I/O error: {message}")]
    public static partial void FileWriteFailed(this ILogger logger, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Result was written to path \"{path}\"")]
    public static partial void FileWriteSuccess(this ILogger logger, string path);
    
    #endregion


    #region Flags

    [LoggerMessage(EventId = 100, Level = LogLevel.Error, Message = "Property of type {propertyType} in class {objectType} can not be parsed")]
    public static partial void PropertyNotParseable(this ILogger logger, Type propertyType, Type objectType);

    [LoggerMessage(EventId = 101, Level = LogLevel.Warning, Message = "Flag \"{flag}\" is used multiple times, ignoring duplicate values")]
    public static partial void DuplicateFlag(this ILogger logger, string flag);

    [LoggerMessage(EventId = 102, Level = LogLevel.Warning, Message = "Unknown flag \"{flag}\"")]
    public static partial void UnknownFlag(this ILogger logger, string flag);

    [LoggerMessage(EventId = 103, Level = LogLevel.Warning, Message = "Value {values} for flag \"{flag}\" is invalid")]
    public static partial void InvalidFlagValue(this ILogger logger, string flag, string[] values);
    
    #endregion


    #region Target

    [LoggerMessage(EventId = 200, Level = LogLevel.Error, Message = "Multiple targets are not supported")]
    public static partial void MultipleTargets(this ILogger logger);

    [LoggerMessage(EventId = 201, Level = LogLevel.Error, Message = "No target was specified")]
    public static partial void NoTarget(this ILogger logger);

    [LoggerMessage(EventId = 202, Level = LogLevel.Error, Message = "Target at \"{path}\" does not exist")]
    public static partial void TargetDoesNotExist(this ILogger logger, string path);

    [LoggerMessage(EventId = 203, Level = LogLevel.Error, Message = "The extension of the target file must be {sourceExtension} or {binaryExtension}")]
    public static partial void TargetExtensionInvalid(this ILogger logger, string sourceExtension, string binaryExtension);

    [LoggerMessage(EventId = 204, Level = LogLevel.Error, Message = "Target path must point to a file, but \"{path}\" is a directory")]
    public static partial void TargetCannotBeDirectory(this ILogger logger, string path);

    #endregion
}