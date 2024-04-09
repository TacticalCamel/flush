namespace Compiler.Data;

internal readonly struct JumpHandle(int index, bool isSource) {
    public int Index { get; } = index;
    public bool IsSource { get; } = isSource;
}