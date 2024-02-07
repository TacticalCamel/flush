namespace ConsoleInterface;

internal sealed class SourceFile(FileInfo fileInfo, byte[] contents) {
    private FileInfo FileInfo { get; } = fileInfo;
    public byte[] Contents { get; } = contents;
    public string Extension => FileInfo.Extension;
    public string FullPath => FileInfo.FullName;
}