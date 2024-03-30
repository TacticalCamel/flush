// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Interpreter.Bytecode;

/// <summary>
/// Represents an identifier for the operation type of an instruction.
/// </summary>
public enum OperationCode : byte {
    /// <summary>Push bytes from the data section to the stack.</summary>
    /// <remarks>opcode:1 size:4 data-address:4</remarks>
    pshd,

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

    /// <summary>Reinterpret the top of the stack from a float to an unsigned integer</summary>
    /// <remarks>opcode:1 float-size:1 uint-size:1</remarks>
    ftou,

    /// <summary>Reinterpret the top of the stack from a float to a signed integer</summary>
    /// <remarks>opcode:1 float-size:1 int-size:1</remarks>
    ftoi,

    /// <summary>Adds the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addi,

    /// <summary>Adds the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addf,

    /// <summary>Subtracts the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    subi,

    /// <summary>Subtracts the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    subf,

    /// <summary>Multiplies the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    muli,

    /// <summary>Multiplies the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    mulf,

    /// <summary>Divides the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    divi,

    /// <summary>Divides the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    divf,
    
    /// <summary>Takes the modulus of the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    modi,
    
    /// <summary>Takes the modulus of the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    modf,
    
    /// <summary>Shifts the bits of a value to the left.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    shfl,
    
    /// <summary>Shifts the bits of a value to the right.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    shfr,
    
    /// <summary>Takes the bitwise 'and' result of the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    and,
    
    /// <summary>Takes the bitwise 'or' result of the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    or,
    
    /// <summary>Takes the bitwise 'xor' result of the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    xor,
    
    /// <summary>Compares the equality 2 top elements of the stack and replaces them with them with the resulting bool value.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    eq,
    
    /// <summary>Compares the inequality 2 top elements of the stack and replaces them with them with the resulting bool value.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    neq,
    
    /// <summary>Increment the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    inci,
    
    /// <summary>Increment the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    incf,
    
    /// <summary>Decrement the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    deci,
    
    /// <summary>Decrement the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    decf,
    
    /// <summary>Swaps the sign of the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    sswi,
    
    /// <summary>Swaps the sign of the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    sswf,
}