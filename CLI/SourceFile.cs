namespace CLI;

using System.Text;
using Interpreter.Serialization;

/// <summary>
/// Represents a file loaded into memory.
/// </summary>
internal sealed class SourceFile {
    /// <summary>
    /// The extension of source files.
    /// </summary>
    public const string SOURCE_FILE_EXTENSION = ".fl";

    /// <summary>
    /// The extension of compiled files.
    /// </summary>
    public const string COMPILED_FILE_EXTENSION = ".flc";

    /// <summary>
    /// The extension of plain text files.
    /// </summary>
    public const string TEXT_FILE_EXTENSION = ".txt";

    /// <summary>
    /// The extension of the file, including the leading dot.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// The contents of the file as an array of bytes.
    /// </summary>
    public byte[] Contents { get; }

    /// <summary>
    /// Represents a file loaded into memory.
    /// </summary>
    /// <param name="file">The file system entry for the source file.</param>
    /// <param name="contents">The contents of the file as an array of bytes.</param>
    private SourceFile(FileSystemInfo file, byte[] contents) {
        Extension = file.Extension;
        FullPath = file.FullName;
        Contents = contents;
    }

    /// <summary>
    /// Attempt to read a file from the file system.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="file">The file entry to read.</param>
    /// <returns>The source file if successful, null otherwise.</returns>
    public static SourceFile? TryRead(ILogger logger, FileInfo file) {
        // file does not exist
        if (!file.Exists) {
            logger.TargetDoesNotExist(file.FullName);
            return null;
        }

        // path is not a file, but a directory
        if ((file.Attributes & FileAttributes.Directory) != 0) {
            logger.TargetCannotBeDirectory(file.FullName);
            return null;
        }

        // read the file contents and return it
        try {
            byte[] contents = File.ReadAllBytes(file.FullName);
            return new SourceFile(file, contents);
        }

        // catch any IO error
        catch (Exception e) {
            logger.GeneralFileError(e.Message);
            return null;
        }
    }

    /// <summary>
    /// Attempt to write a compiled script to the file system.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="script">The compiled script.</param>
    /// <param name="sourceFile">The source file the script was created from.</param>
    /// <param name="outputPath">The path to write the file to.</param>
    /// <param name="compileToPlainText">Whether the output contents should be in plain text.</param>
    public static void TryWrite(ILogger logger, Script script, SourceFile sourceFile, string? outputPath, bool compileToPlainText) {
        // if no custom output path is provided, put the file to the same directory as the input
        string filePath = outputPath ?? sourceFile.FullPath;

        // get file extension
        string fileExtension = compileToPlainText ? TEXT_FILE_EXTENSION : COMPILED_FILE_EXTENSION;

        // correct the file extension
        filePath = Path.ChangeExtension(filePath, fileExtension);

        try {
            // create and open the file
            FileStream fileStream = new(filePath, FileMode.Create);

            // write plain text
            if (compileToPlainText) {
                StreamWriter streamWriter = new(fileStream, Encoding.UTF8);
                script.WriteStringContents(streamWriter);
                streamWriter.Flush();
            }

            // write binary
            else {
                byte[] bytes = BinarySerializer.ScriptToBytes(script);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
            }

            logger.FileWriteSuccess(filePath);
        }

        // catch any IO error
        catch (Exception e) {
            logger.GeneralFileError(e.Message);
        }
    }
}