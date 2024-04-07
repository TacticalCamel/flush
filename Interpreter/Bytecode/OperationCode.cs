// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Interpreter.Bytecode;

/// <summary>
/// Represents an identifier for the operation type of an instruction.
/// </summary>
public enum OperationCode : byte {
    /// <summary>Stop the program execution.</summary>
    /// <remarks>opcode:1 return-code:4</remarks>
    exit,
    
    /// <summary>Set the instruction pointer.</summary>
    /// <remarks>opcode:1 value:4</remarks>
    jump,
    
    /// <summary>Set the instruction pointer if the top byte of the stack evaluates to true. Removes the byte.</summary>
    /// <remarks>opcode:1 value:4</remarks>
    cjmp,

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

    /// <summary>Add the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addi,

    /// <summary>Add the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    addf,

    /// <summary>Subtract the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    subi,

    /// <summary>Subtract the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    subf,

    /// <summary>Multiply the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    muli,

    /// <summary>Multiply the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    mulf,

    /// <summary>Divide the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    divi,

    /// <summary>Divide the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    divf,

    /// <summary>Take the modulus of the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    modi,

    /// <summary>Take the modulus of the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    modf,

    /// <summary>Shift the bits of a value to the left.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    shfl,

    /// <summary>Shift the bits of a value to the right.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    shfr,

    /// <summary>Perform the bitwise 'and' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    and,

    /// <summary>Perform the bitwise 'or' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    or,

    /// <summary>Perform the bitwise 'xor' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    xor,

    /// <summary>Perform the 'equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    eq,

    /// <summary>Perform the 'not equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    neq,

    /// <summary>Perform the 'less' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    lt,

    /// <summary>Perform the 'less or equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    lte,

    /// <summary>Perform the 'greater than' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    gt,

    /// <summary>Perform the 'greater or equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:1</remarks>
    gte,

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

    /// <summary>Negates the top element of the stack as a bool.</summary>
    /// <remarks>opcode:1</remarks>
    negb,
}