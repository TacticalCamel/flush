namespace CLI.IO;

using System.Text;
using Interpreter.Serialization;

/// <summary>
/// Class containing static methods for file operations.
/// </summary>
internal static class FileOperations {
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
            logger.FileWriteFailed(e.Message);
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
        string fileExtension = compileToPlainText ? SourceFile.TEXT_FILE_EXTENSION : SourceFile.COMPILED_FILE_EXTENSION;

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
            logger.FileWriteFailed(e.Message);
        }
    }
}