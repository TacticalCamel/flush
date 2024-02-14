namespace ConsoleInterface;

internal sealed class SourceFile(FileInfo fileInfo, byte[] contents) {
    public const string FILE_SOURCE_EXTENSION = ".sra";
    public const string FILE_BINARY_EXTENSION = ".bin";
    public const string FILE_TEXT_EXTENSION = ".txt";
    
    public string Extension { get; } = fileInfo.Extension;
    public string FullPath { get; } = fileInfo.FullName;
    public byte[] Contents { get; } = contents;
}