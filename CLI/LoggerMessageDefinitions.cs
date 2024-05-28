namespace CLI;

/// <summary>
/// Logger message templates to be used by the CLI.
/// </summary>
internal static partial class LoggerMessageDefinitions {
    #region General

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Started application with arguments: [{args}]")]
    public static partial void ApplicationStarted(this ILogger logger, string[] args);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "File error: {message}")]
    public static partial void GeneralFileError(this ILogger logger, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Result was written to path \"{path}\"")]
    public static partial void FileWriteSuccess(this ILogger logger, string path);

    #endregion

    #region Target
    
    [LoggerMessage(EventId = 202, Level = LogLevel.Error, Message = "Target at \"{path}\" does not exist")]
    public static partial void TargetDoesNotExist(this ILogger logger, string path);

    [LoggerMessage(EventId = 203, Level = LogLevel.Error, Message = "The extension of the target file must be {sourceExtension} or {binaryExtension}")]
    public static partial void TargetExtensionInvalid(this ILogger logger, string sourceExtension, string binaryExtension);

    [LoggerMessage(EventId = 204, Level = LogLevel.Error, Message = "Target path must point to a file, but \"{path}\" is a directory")]
    public static partial void TargetCannotBeDirectory(this ILogger logger, string path);

    #endregion
}