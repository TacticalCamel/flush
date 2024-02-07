namespace ConsoleInterface;

internal sealed class SourceFile(FileInfo fileInfo, byte[] contents) {
    public const string FILE_SOURCE_EXTENSION = ".sra";
    public const string FILE_BINARY_EXTENSION = ".bin";
    public const string FILE_TEXT_EXTENSION = ".txt";
    
    private FileInfo FileInfo { get; } = fileInfo;
    
    public byte[] Contents { get; } = contents;
    public string Extension => FileInfo.Extension;
    public string FullPath => FileInfo.FullName;
}