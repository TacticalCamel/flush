namespace Compiler.Data;

/// <summary>
/// Represents an incomplete instruction jump.
/// </summary>
/// <param name="index">The index in the instruction list the handle points to.</param>
/// <param name="isSource">Whether the handle points to the source or the destination of the jump.</param>
internal readonly struct JumpHandle(int index, bool isSource) {
    /// <summary>
    /// The index in the instruction list the handle points to.
    /// </summary>
    public int Index { get; } = index;
    
    /// <summary>
    /// Whether the handle points to the source or the destination of the jump.
    /// </summary>
    public bool IsSource { get; } = isSource;
}