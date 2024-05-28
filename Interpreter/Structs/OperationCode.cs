// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Interpreter.Structs;

/// <summary>
/// Represents an identifier for the operation type of an instruction.
/// </summary>
public enum OperationCode : byte {
    /// <summary>Stop the program execution.</summary>
    /// <remarks>opcode:1 return-code:4</remarks>
    exit,
    
    /// <summary>Pause the program execution until user input.</summary>
    /// <remarks>opcode:1</remarks>
    dbug,
    
    /// <summary>Set the instruction pointer.</summary>
    /// <remarks>opcode:1 value:4</remarks>
    jump,
    
    /// <summary>Set the instruction pointer if the top byte of the stack evaluates to true. Removes the byte.</summary>
    /// <remarks>opcode:1 value:4</remarks>
    cjmp,

    /// <summary>Push bytes from the data section to the stack.</summary>
    /// <remarks>opcode:1 size:4 data-address:4</remarks>
    pshd,
    
    /// <summary>Copy bytes from the stack to the top of the stack.</summary>
    /// <remarks>opcode:1 size:4 stack-address:4</remarks>
    pshs,
    
    /// <summary>Copy bytes from the top of the stack to another location in the stack.</summary>
    /// <remarks>opcode:1 size:4 stack-address:4</remarks>
    asgm,
    
    /// <summary>Push a number of 0 bytes to the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    pshz,

    /// <summary>Pop a number of bytes from the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    pop,

    /// <summary>Reinterpret the top of the stack from a signed integer to a float</summary>
    /// <remarks>opcode:1 int-size:2 float-size:2</remarks>
    itof,

    /// <summary>Reinterpret the top of the stack from an unsigned integer to a float</summary>
    /// <remarks>opcode:1 uint-size:2 float-size:2</remarks>
    utof,

    /// <summary>Reinterpret the top of the stack from one float type to another</summary>
    /// <remarks>opcode:1 float-size:2 float-size:2</remarks>
    ftof,

    /// <summary>Reinterpret the top of the stack from a float to an unsigned integer</summary>
    /// <remarks>opcode:1 float-size:2 uint-size:2</remarks>
    ftou,

    /// <summary>Reinterpret the top of the stack from a float to a signed integer</summary>
    /// <remarks>opcode:1 float-size:2 int-size:2</remarks>
    ftoi,

    /// <summary>Add the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    addi,

    /// <summary>Add the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    addf,

    /// <summary>Subtract the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    subi,

    /// <summary>Subtract the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    subf,

    /// <summary>Multiply the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    muli,

    /// <summary>Multiply the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    mulf,

    /// <summary>Divide the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    divi,

    /// <summary>Divide the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    divf,

    /// <summary>Take the modulus of the 2 top elements of the stack as integers.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    modi,

    /// <summary>Take the modulus of the 2 top elements of the stack as floats.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    modf,

    /// <summary>Shift the bits of a value to the left.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    shfl,

    /// <summary>Shift the bits of a value to the right.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    shfr,

    /// <summary>Perform the bitwise 'and' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    and,

    /// <summary>Perform the bitwise 'or' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    or,

    /// <summary>Perform the bitwise 'xor' operation on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    xor,

    /// <summary>Perform the 'equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    eq,

    /// <summary>Perform the 'not equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    neq,

    /// <summary>Perform the 'less' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    lt,

    /// <summary>Perform the 'less or equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    lte,

    /// <summary>Perform the 'greater than' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    gt,

    /// <summary>Perform the 'greater or equal' comparison on the 2 top elements of the stack.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    gte,

    /// <summary>Increment the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    inci,

    /// <summary>Increment the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    incf,

    /// <summary>Decrement the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    deci,

    /// <summary>Decrement the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    decf,

    /// <summary>Swaps the sign of the top element of the stack as an integer.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    sswi,

    /// <summary>Swaps the sign of the top element of the stack as an float.</summary>
    /// <remarks>opcode:1 size:2</remarks>
    sswf,

    /// <summary>Negates the top element of the stack as a bool.</summary>
    /// <remarks>opcode:1</remarks>
    negb,
}