namespace Compiler.Data;

internal readonly struct JumpHandle(int index) {
    public int Index { get; } = index;
}