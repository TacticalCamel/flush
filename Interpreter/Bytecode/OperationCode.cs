// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Interpreter.Bytecode;

/// <summary>
/// Represents an identifier for the operation type of an instruction.
/// </summary>
public enum OperationCode: byte {
    /// <summary>Push bytes from the data section to the stack.</summary>
    /// <remarks>opcode:1 size:4 data-address:4</remarks>
    pshd,
    
    /// <summary>Adds the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addi,
    
    /// <summary>Adds the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addf,
    
    /// <summary>Push a number of 0 bytes to the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    pshz,
    
    /// <summary>Pop a number of bytes from the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    pop,
    
    /// <summary>Reinterpret the top of the stack from a signed integer to a float</summary>
    /// <remarks>opcode:1 int-size:1 float-size:1</remarks>
    itof,
    
    /// <summary>Reinterpret the top of the stack from an unsigned integer to a float</summary>
    /// <remarks>opcode:1 uint-size:1 float-size:1</remarks>
    utof,
    
    /// <summary>Reinterpret the top of the stack from one float type to another</summary>
    /// <remarks>opcode:1 float-size:1 float-size:1</remarks>
    ftof,
}