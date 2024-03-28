namespace Compiler.Data;

/// <summary>
/// Represents a location where data can be accessed by the program.
/// </summary>
internal enum MemoryLocation : byte {
    /// <summary>
    /// The location of the data section.
    /// This is for constants that are compiled into the executable.
    /// </summary>
    Data,

    /// <summary>
    /// The location of the stack during runtime.
    /// Value types can be allocated here.
    /// </summary>
    Stack,

    /// <summary>
    /// The location of the heap during runtime.
    /// Reference types can be allocated here.
    /// </summary>
    Heap
}