namespace Interpreter.Bytecode;

/// <summary>
/// Represents an atomic operation that the interpreter can execute.
/// </summary>
/// <remarks>
/// This struct has an explicit layout with overlapping fields,
/// which must not be used at the same time.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public unsafe struct Instruction {
    /// <summary>
    /// The raw byte values of the instruction.
    /// </summary>
    [FieldOffset(0)]
    public fixed byte Bytes[8];

    /// <summary>
    /// The instruction identifier.
    /// Always used.
    /// </summary>
    [FieldOffset(7)]
    public OperationCode Code;

    /// <summary>
    /// The address of a value in the data section.
    /// Used when copying from the data section.
    /// </summary>
    [FieldOffset(0)]
    public int DataAddress;
    
    /// <summary>
    /// The size of a second type in bytes.
    /// Used for primitive casts.
    /// </summary>
    [FieldOffset(0)]
    public ushort SecondTypeSize;

    /// <summary>
    /// The size of a type in bytes.
    /// Used for primitive operations and casts.
    /// </summary>
    [FieldOffset(4)]
    public ushort TypeSize;
}