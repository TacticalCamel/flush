namespace CLI;

/// <summary>
/// Represents a file loaded into memory.
/// </summary>
/// <param name="file">The file system entry for the source file.</param>
/// <param name="contents">The contents of the file as an array of bytes.</param>
internal sealed class SourceFile(FileSystemInfo file, byte[] contents) {
    /// <summary>
    /// The valid extension for source files.
    /// </summary>
    public const string FILE_SOURCE_EXTENSION = ".sra";

    /// <summary>
    /// The valid extension for compiled files.
    /// </summary>
    public const string FILE_BINARY_EXTENSION = ".bin";

    /// <summary>
    /// The valid extension for plain text files.
    /// </summary>
    public const string FILE_TEXT_EXTENSION = ".txt";

    /// <summary>
    /// The extension of the file, including the leading dot.
    /// </summary>
    public string Extension { get; } = file.Extension;

    /// <summary>
    /// The full path of the file.
    /// </summary>
    public string FullPath { get; } = file.FullName;

    /// <summary>
    /// The contents of the file as an array of bytes.
    /// </summary>
    public byte[] Contents { get; } = contents;
}