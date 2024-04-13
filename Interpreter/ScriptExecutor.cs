// it's true that nothing prevents the stack allocated array from being returned,
// but we won't do that, so it's okay

#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside the containing method

namespace Interpreter;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bytecode;

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
    /// The stack memory of the program.
    /// </summary>
    private readonly Span<byte> Stack;

    /// <summary>
    /// The index of the currently executed instruction.
    /// </summary>
    private int InstructionIndex;

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

        // debug info
        HexLength = Math.Max(2, Instructions.Length.ToString("X").Length);
        StrLength = HexLength + 5;
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
            // logical operations

            case OperationCode.exit:
                DebugState($"{stopwatch.Elapsed.TotalMilliseconds:N3}ms");
                return;
            
            case OperationCode.dbug:
                DebugState("PAUSED");
                stopwatch.Stop();
                Console.ReadKey();
                stopwatch.Start();
                break;

            case OperationCode.jump:
                DebugState($"0x{i.Address:X}");

                InstructionIndex = i.Address;

                goto start;

            case OperationCode.cjmp:
                StackPtr--;

                DebugState($"0x{i.Address:X}");

                if (*StackPtr == 0) {
                    InstructionIndex = i.Address;
                    goto start;
                }

                break;

            // stack operations

            case OperationCode.pshd: {
                fixed (byte* source = &Data[i.Address]) {
                    Unsafe.CopyBlockUnaligned(StackPtr, source, i.TypeSize);
                }

                StackPtr += i.TypeSize;

                DebugState($"0x{i.Address:X} {i.TypeSize}");
                break;
            }

            case OperationCode.pshz: {
                Unsafe.InitBlockUnaligned(StackPtr, 0, i.Count);
                StackPtr += i.Count;

                DebugState($"{i.Count}");
                break;
            }

            case OperationCode.pop: {
                StackPtr -= i.Count;

                DebugState($"{i.Count}");
                break;
            }
            
            case OperationCode.pshs:
                fixed (byte* source = &Stack[i.Address]) {
                    Unsafe.CopyBlockUnaligned(StackPtr, source, i.TypeSize);
                }
                
                StackPtr += i.TypeSize;
                
                DebugState($"0x{i.Address:X} {i.TypeSize}");
                break;
            
            case OperationCode.asgm:
                StackPtr -= i.TypeSize;
                
                fixed (byte* destination = &Stack[i.Address]) {
                    Unsafe.CopyBlockUnaligned(destination, StackPtr, i.TypeSize);
                }
                
                DebugState($"0x{i.Address:X} {i.TypeSize}");
                break;

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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
                break;
            }

            case OperationCode.negb: {
                *(StackPtr - 1) = (byte)(~(*(StackPtr - 1)) & 1);

                DebugState($"{i.Code}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize}");
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

                DebugState($"{i.TypeSize} {i.SecondTypeSize}");
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

                DebugState($"{i.TypeSize} {i.SecondTypeSize}");
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

                DebugState($"{i.TypeSize} {i.SecondTypeSize}");
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

                DebugState($"{i.TypeSize} {i.SecondTypeSize}");
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

                DebugState($"{i.TypeSize} {i.SecondTypeSize}");
                break;

            default:
                throw new ArgumentException($"Interpreter could not execute instruction '{i.Code}'");
        }

        // move to the next instruction
        InstructionIndex++;
        goto start;
    }

    #region Debug information

    private readonly int HexLength;
    private readonly int StrLength;

    private void DebugState(string message) {
        int index = (int)(StackPtr - (byte*)Unsafe.AsPointer(ref Stack[0]));
        string stack = string.Join(' ', Stack[..index].ToArray().Select(x => x.ToString("X2")));

        Console.WriteLine($"0x{InstructionIndex.ToString("X").PadLeft(HexLength, '0')} | {Instructions[InstructionIndex].Code,-4} {message.PadRight(StrLength)} | {stack}");
    }

    #endregion
}