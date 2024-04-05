// it's true that nothing prevents the stack allocated array from being returned
// but we won't do that so it's okay

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Interpreter.Bytecode;

#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside of the containing method

namespace Interpreter;

/// <summary>
/// Implements a virtual processor that is capable of executing instructions. 
/// </summary>
public unsafe ref struct ScriptExecutor {
    /// <summary>
    /// The data section of the program.
    /// </summary>
    private readonly ReadOnlySpan<byte> Data;

    /// <summary>
    /// The instructions of the program.
    /// </summary>
    private readonly ReadOnlySpan<Instruction> Instructions;

    /// <summary>
    /// The index of the currently executed instruction.
    /// </summary>
    private int InstructionIndex;

    /// <summary>
    /// The stack memory of the program.
    /// </summary>
    private readonly Span<byte> Stack;

    /// <summary>
    /// The first free byte in the stack.
    /// </summary>
    private byte* StackPtr;

    /// <summary>
    /// Create a new script executor.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    public ScriptExecutor(Script script) {
        Data = script.Data.Span;
        Instructions = script.Instructions.Span;
        
        // default stack size is 256kb
        Stack = stackalloc byte[1 << 18];

        // stack allocated memory, doesn't have to be pinned
        StackPtr = (byte*)Unsafe.AsPointer(ref Stack[0]);
    }

    /// <summary>
    /// Executes the program.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when an unknown instruction is encountered.</exception>
    public void Run() {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // start label to jump back to
        start:
        
        // the current instruction
        Instruction i = Instructions[InstructionIndex];

        // the horrors persist
        // but so do i
        switch (i.Code) {
            case OperationCode.exit:
                Console.WriteLine($"executed in {stopwatch.Elapsed.TotalMilliseconds:N3}ms");
                return;
            
            // stack operations

            case OperationCode.pshd: {
                fixed (byte* source = &Data[i.DataAddress]) {
                    Unsafe.CopyBlockUnaligned(StackPtr, source, i.TypeSize);
                }

                StackPtr += i.TypeSize;

                Console.WriteLine($"{i.Code} addr=0x{i.DataAddress:X} size={i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.pshz: {
                Unsafe.InitBlockUnaligned(StackPtr, 0, i.TypeSize);
                StackPtr += i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.pop: {
                StackPtr -= i.Count;

                Console.WriteLine($"{i.Code} {i.Count}\n    {StackString}\n");
                break;
            }

            // integer operations

            case OperationCode.addi: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) += *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) += *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) += *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) += *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) += *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.subi: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) -= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) -= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) -= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) -= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) -= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.muli: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) *= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) *= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) *= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) *= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) *= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.divi: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) /= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) /= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) /= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) /= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) /= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.modi: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) %= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) %= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) %= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) %= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) %= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.inci: {
                switch (i.TypeSize) {
                    case 1:
                        (*(StackPtr - 1))++;
                        break;
                    case 2:
                        (*((ushort*)StackPtr - 1))++;
                        break;
                    case 4:
                        (*((uint*)StackPtr - 1))++;
                        break;
                    case 8:
                        (*((ulong*)StackPtr - 1))++;
                        break;
                    case 16:
                        (*((UInt128*)StackPtr - 1))++;
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.deci: {
                switch (i.TypeSize) {
                    case 1:
                        (*(StackPtr - 1))--;
                        break;
                    case 2:
                        (*((ushort*)StackPtr - 1))--;
                        break;
                    case 4:
                        (*((uint*)StackPtr - 1))--;
                        break;
                    case 8:
                        (*((ulong*)StackPtr - 1))--;
                        break;
                    case 16:
                        (*((UInt128*)StackPtr - 1))--;
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.sswi: {
                switch (i.TypeSize) {
                    case 1:
                        *((sbyte*)StackPtr - 1) = (sbyte)-*((sbyte*)StackPtr - 1);
                        break;
                    case 2:
                        *((short*)StackPtr - 1) = (short)-*((short*)StackPtr - 1);
                        break;
                    case 4:
                        *((int*)StackPtr - 1) = -*((int*)StackPtr - 1);
                        break;
                    case 8:
                        *((long*)StackPtr - 1) = -*((long*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 1) = -*((UInt128*)StackPtr - 1);
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.shfl: {
                int* ptr = (int*)StackPtr - 1;

                switch (i.TypeSize) {
                    case 1: {
                        *((byte*)ptr - 1) <<= *ptr;
                        break;
                    }
                    case 2: {
                        *((ushort*)ptr - 1) <<= *ptr;
                        break;
                    }
                    case 4: {
                        *((uint*)ptr - 1) <<= *ptr;
                        break;
                    }
                    case 8: {
                        *((ulong*)ptr - 1) <<= *ptr;
                        break;
                    }
                    case 16: {
                        *((UInt128*)ptr - 1) <<= *ptr;
                        break;
                    }
                }

                StackPtr -= 4;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.shfr: {
                int* ptr = (int*)StackPtr - 1;

                switch (i.TypeSize) {
                    case 1:
                        *((byte*)ptr - 1) >>= *ptr;
                        break;
                    case 2:
                        *((ushort*)ptr - 1) >>= *ptr;
                        break;
                    case 4:
                        *((uint*)ptr - 1) >>= *ptr;
                        break;
                    case 8:
                        *((ulong*)ptr - 1) >>= *ptr;
                        break;
                    case 16:
                        *((UInt128*)ptr - 1) >>= *ptr;
                        break;
                }

                StackPtr -= 4;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            // float operations

            case OperationCode.addf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 2) += *((Half*)StackPtr - 1);
                        break;
                    case 4:
                        (*((float*)StackPtr - 2)) += *((float*)StackPtr - 1);
                        break;
                    case 8:
                        (*((double*)StackPtr - 2)) += *((double*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.subf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 2) -= *((Half*)StackPtr - 1);
                        break;
                    case 4:
                        (*((float*)StackPtr - 2)) -= *((float*)StackPtr - 1);
                        break;
                    case 8:
                        (*((double*)StackPtr - 2)) -= *((double*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.mulf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 2) *= *((Half*)StackPtr - 1);
                        break;
                    case 4:
                        (*((float*)StackPtr - 2)) *= *((float*)StackPtr - 1);
                        break;
                    case 8:
                        (*((double*)StackPtr - 2)) *= *((double*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.divf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 2) /= *((Half*)StackPtr - 1);
                        break;
                    case 4:
                        (*((float*)StackPtr - 2)) /= *((float*)StackPtr - 1);
                        break;
                    case 8:
                        (*((double*)StackPtr - 2)) /= *((double*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.modf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 2) %= *((Half*)StackPtr - 1);
                        break;
                    case 4:
                        (*((float*)StackPtr - 2)) %= *((float*)StackPtr - 1);
                        break;
                    case 8:
                        (*((double*)StackPtr - 2)) %= *((double*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.incf: {
                switch (i.TypeSize) {
                    case 2:
                        (*((Half*)StackPtr - 1))++;
                        break;
                    case 4:
                        (*((float*)StackPtr - 1))++;
                        break;
                    case 8:
                        (*((double*)StackPtr - 1))++;
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.decf: {
                switch (i.TypeSize) {
                    case 2:
                        (*((Half*)StackPtr - 1))--;
                        break;
                    case 4:
                        (*((float*)StackPtr - 1))--;
                        break;
                    case 8:
                        (*((double*)StackPtr - 1))--;
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.sswf: {
                switch (i.TypeSize) {
                    case 2:
                        *((Half*)StackPtr - 1) = -*((Half*)StackPtr - 1);
                        break;
                    case 4:
                        *((float*)StackPtr - 1) = -*((float*)StackPtr - 1);
                        break;
                    case 8:
                        *((double*)StackPtr - 1) = -*((double*)StackPtr - 1);
                        break;
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            // bit operations

            case OperationCode.and: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) &= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) &= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) &= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) &= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) &= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.or: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) |= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) |= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) |= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) |= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) |= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.xor: {
                switch (i.TypeSize) {
                    case 1:
                        *(StackPtr - 2) ^= *(StackPtr - 1);
                        break;
                    case 2:
                        *((ushort*)StackPtr - 2) ^= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *((uint*)StackPtr - 2) ^= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *((ulong*)StackPtr - 2) ^= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *((UInt128*)StackPtr - 2) ^= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.negb: {
                *(StackPtr - 1) = (byte)(~(*(StackPtr - 1)) & 1);

                Console.WriteLine($"{i.Code}\n    {StackString}\n");
                break;
            }

            // comparison operations

            case OperationCode.eq: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) == *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) == *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) == *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) == *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) == *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            case OperationCode.neq: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) != *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) != *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) != *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) != *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) != *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }
            
            case OperationCode.lt: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) < *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) < *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) < *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) < *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) < *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }
            
            case OperationCode.lte: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) <= *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) <= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) <= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) <= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) <= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }
            
            case OperationCode.gt: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) > *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) > *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) > *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) > *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) > *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }
            
            case OperationCode.gte: {
                switch (i.TypeSize) {
                    case 1:
                        *(bool*)(StackPtr - 2) = *(StackPtr - 2) >= *(StackPtr - 1);
                        break;
                    case 2:
                        *(bool*)((ushort*)StackPtr - 2) = *((ushort*)StackPtr - 2) >= *((ushort*)StackPtr - 1);
                        break;
                    case 4:
                        *(bool*)((uint*)StackPtr - 2) = *((uint*)StackPtr - 2) >= *((uint*)StackPtr - 1);
                        break;
                    case 8:
                        *(bool*)((ulong*)StackPtr - 2) = *((ulong*)StackPtr - 2) >= *((ulong*)StackPtr - 1);
                        break;
                    case 16:
                        *(bool*)((UInt128*)StackPtr - 2) = *((UInt128*)StackPtr - 2) >= *((UInt128*)StackPtr - 1);
                        break;
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;
            }

            // casts

            case OperationCode.itof:
                switch (i.TypeSize) {
                    case 1: {
                        sbyte* ptr = (sbyte*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = *ptr;
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 2: {
                        short* ptr = (short*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 4: {
                        int* ptr = (int*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 8: {
                        long* ptr = (long*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.utof:
                switch (i.TypeSize) {
                    case 1: {
                        byte* ptr = StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = *ptr;
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 2: {
                        ushort* ptr = (ushort*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 4: {
                        uint* ptr = (uint*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 8: {
                        ulong* ptr = (ulong*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = *ptr;
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftof:
                switch (i.TypeSize) {
                    case 2: {
                        Half* ptr = (Half*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 4:
                                *(float*)ptr = (float)*ptr;
                                break;
                            case 8:
                                *(double*)ptr = (double)*ptr;
                                break;
                        }

                        break;
                    }
                    case 4: {
                        float* ptr = (float*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 8:
                                *(double*)ptr = *ptr;
                                break;
                        }

                        break;
                    }
                    case 8: {
                        double* ptr = (double*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 2:
                                *(Half*)ptr = (Half)(*ptr);
                                break;
                            case 4:
                                *(float*)ptr = (float)*ptr;
                                break;
                        }

                        break;
                    }
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftoi:
                switch (i.TypeSize) {
                    case 2: {
                        Half* ptr = (Half*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(sbyte*)ptr = (sbyte)*ptr;
                                break;
                            case 2:
                                *(short*)ptr = (short)*ptr;
                                break;
                            case 4:
                                *(int*)ptr = (int)*ptr;
                                break;
                            case 8:
                                *(long*)ptr = (long)*ptr;
                                break;
                        }

                        break;
                    }
                    case 4: {
                        float* ptr = (float*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(sbyte*)ptr = (sbyte)*ptr;
                                break;
                            case 2:
                                *(short*)ptr = (short)*ptr;
                                break;
                            case 4:
                                *(int*)ptr = (int)*ptr;
                                break;
                            case 8:
                                *(long*)ptr = (long)*ptr;
                                break;
                        }

                        break;
                    }
                    case 8: {
                        double* ptr = (double*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(sbyte*)ptr = (sbyte)*ptr;
                                break;
                            case 2:
                                *(short*)ptr = (short)*ptr;
                                break;
                            case 4:
                                *(int*)ptr = (int)*ptr;
                                break;
                            case 8:
                                *(long*)ptr = (long)*ptr;
                                break;
                        }

                        break;
                    }
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftou:
                switch (i.TypeSize) {
                    case 2: {
                        Half* ptr = (Half*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(byte*)ptr = (byte)*ptr;
                                break;
                            case 2:
                                *(ushort*)ptr = (ushort)*ptr;
                                break;
                            case 4:
                                *(uint*)ptr = (uint)*ptr;
                                break;
                            case 8:
                                *(ulong*)ptr = (ulong)*ptr;
                                break;
                        }

                        break;
                    }
                    case 4: {
                        float* ptr = (float*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(byte*)ptr = (byte)*ptr;
                                break;
                            case 2:
                                *(ushort*)ptr = (ushort)*ptr;
                                break;
                            case 4:
                                *(uint*)ptr = (uint)*ptr;
                                break;
                            case 8:
                                *(ulong*)ptr = (ulong)*ptr;
                                break;
                        }

                        break;
                    }
                    case 8: {
                        double* ptr = (double*)StackPtr - 1;

                        switch (i.SecondTypeSize) {
                            case 1:
                                *(byte*)ptr = (byte)*ptr;
                                break;
                            case 2:
                                *(ushort*)ptr = (ushort)*ptr;
                                break;
                            case 4:
                                *(uint*)ptr = (uint)*ptr;
                                break;
                            case 8:
                                *(ulong*)ptr = (ulong)*ptr;
                                break;
                        }

                        break;
                    }
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            default:
                throw new ArgumentException($"Interpreter could not execute instruction '{i.Code}'");
        }

        // move to the next instruction
        InstructionIndex++;
        goto start;
    }

    /// <summary>
    /// Debug property: return the current state of the stack.
    /// </summary>
    private string StackString {
        get { return $"stack: {string.Join(' ', Stack[..(int)(StackPtr - (byte*)Unsafe.AsPointer(ref Stack[0]))].ToArray().Select(x => $"{x:X2}"))}"; }
    }
}