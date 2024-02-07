namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MetaData() {
    private const ulong FILE_HEADER = 0x67FF67FF00617273;

    public ulong Header { get; } = FILE_HEADER;
    public required ulong Version { get; init; }
    public required DateTime CompilationTime { get; init; }
    public required int DataOffset { get; init; }
    public required int CodeOffset { get; init; }

    public bool IsHeaderValid => Header == FILE_HEADER;
}