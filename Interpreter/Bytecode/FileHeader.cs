﻿namespace Interpreter.Bytecode;

/// <summary>
/// Represents the header of a compiled program.
/// Contains deserialization information and metadata.
/// </summary>
public readonly struct FileHeader {
    /// <summary>
    /// The valid signature value.
    /// </summary>
    public const ulong VALID_SIGNATURE = 0x_42FF_54FF_0061_7273;

    /// <summary>
    /// The signature of the header.
    /// Usually a constant value that serves as basic validation.
    /// </summary>
    public required ulong Signature { get; init; }

    /// <summary>
    /// The bytecode version of the program.
    /// </summary>
    public required ulong Version { get; init; }

    /// <summary>
    /// The timestamp when the program was created.
    /// </summary>
    public required DateTime CompilationTime { get; init; }

    /// <summary>
    /// The index of the first byte of the program data.
    /// </summary>
    public required int DataStart { get; init; }

    /// <summary>
    /// The index of the first byte of the program instructions.
    /// </summary>
    public required int CodeStart { get; init; }
}