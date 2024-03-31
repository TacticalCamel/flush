// it's true that nothing prevents the stack allocated array from being returned
// but we won't do that so it's okay

using System.Runtime.CompilerServices;

#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside of the containing method

namespace Interpreter;

using System.Diagnostics;
using Serialization;
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
    /// The stack memory of the program. Default size is 256kb.
    /// </summary>
    private readonly Span<byte> Stack;
    
    /// <summary>
    /// The index of the currently executed instruction.
    /// </summary>
    private int InstructionPtr;

    /// <summary>
    /// The index of the first free byte in the stack, same as the current size of the stack.
    /// </summary>
    private int StackPtr;

    /// <summary>
    /// Create a new script executor.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    public ScriptExecutor(Script script) {
        Data = script.Data.Span;
        Instructions = script.Instructions.Span;
        Stack = stackalloc byte[1 << 18];

        //StackPtr = (byte*)Unsafe.AsPointer(ref Stack[0]);
    }

    /// <summary>
    /// Executes the program.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when an unknown instruction is encountered.</exception>
    public void Run() {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // start label to jump back to
        start:

        // return if reached the end
        if (InstructionPtr >= Instructions.Length) {
            Console.WriteLine($"executed in {stopwatch.Elapsed.TotalMilliseconds:N3}ms");
            return;
        }

        // the current instruction
        Instruction i = Instructions[InstructionPtr];

        // the horrors persist
        // but so do i
        switch (i.Code) {
            // stack operations

            case OperationCode.pshd:
                Data[i.DataAddress..(i.DataAddress + i.TypeSize)].CopyTo(Stack[StackPtr..]);
                StackPtr += i.TypeSize;

                Console.WriteLine($"{i.Code} addr=0x{i.DataAddress:X} size={i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.pshz:
                Stack[StackPtr..(StackPtr + i.TypeSize)].Clear();
                StackPtr += i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.pop:
                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            // integer operations

            case OperationCode.addi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) + *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) + *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.subi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) - *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) - *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.muli:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) * *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) * *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.divi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) / *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) / *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.modi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) % *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) % *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.inci:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            (*(bytePtr - 1))++;
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.deci:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            (*(bytePtr - 1))--;
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.sswi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1: {
                            sbyte* ptr = (sbyte*)bytePtr;
                            *(ptr - 1) = (sbyte)-*(ptr - 1);
                            break;
                        }
                        case 2: {
                            short* ptr = (short*)bytePtr;
                            *(ptr - 1) = (short)-*(ptr - 1);
                            break;
                        }
                        case 4: {
                            int* ptr = (int*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                        case 8: {
                            long* ptr = (long*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                        case 16: {
                            Int128* ptr = (Int128*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.shfl:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    int* ptr = (int*)bytePtr - 1;

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
                }

                StackPtr -= 4;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.shfr:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    int* ptr = (int*)bytePtr - 1;

                    switch (i.TypeSize) {
                        case 1: {
                            *((byte*)ptr - 1) >>= *ptr;
                            break;
                        }
                        case 2: {
                            *((ushort*)ptr - 1) >>= *ptr;
                            break;
                        }
                        case 4: {
                            *((uint*)ptr - 1) >>= *ptr;
                            break;
                        }
                        case 8: {
                            *((ulong*)ptr - 1) >>= *ptr;
                            break;
                        }
                        case 16: {
                            *((UInt128*)ptr - 1) >>= *ptr;
                            break;
                        }
                    }
                }

                StackPtr -= 4;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            // float operations

            case OperationCode.addf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 2)) += *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.subf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 2)) -= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.mulf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 2)) *= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.divf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 2)) /= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.modf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 2)) %= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.incf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 1))++;
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.decf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            (*(ptr - 1))--;
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.sswf:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                        case 4: {
                            float* ptr = (float*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                        case 8: {
                            double* ptr = (double*)bytePtr;
                            *(ptr - 1) = -*(ptr - 1);
                            break;
                        }
                    }
                }

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            // bit operations

            case OperationCode.and:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) & *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) & *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) &= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) &= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) &= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.or:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) | *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) | *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) |= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) |= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) |= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.xor:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bytePtr - 2) = (byte)(*(bytePtr - 2) ^ *(bytePtr - 1));
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(ptr - 2) = (ushort)(*(ptr - 2) ^ *(ptr - 1));
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            (*(ptr - 2)) ^= *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            (*(ptr - 2)) ^= *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            (*(ptr - 2)) ^= *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.negb:
                fixed (byte* ptr = &Stack[StackPtr]) {
                    *(ptr - 1) = (byte)(~(*(ptr - 1)) & 1);
                }

                Console.WriteLine($"{i.Code}\n    {StackString}\n");
                break;

            // comparison operations

            case OperationCode.eq:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bool*)(bytePtr - 2) = *(bytePtr - 2) == *(bytePtr - 1);
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) == *(ptr - 1);
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) == *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) == *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) == *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.neq:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1:
                            *(bool*)(bytePtr - 2) = *(bytePtr - 2) != *(bytePtr - 1);
                            break;
                        case 2: {
                            ushort* ptr = (ushort*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) != *(ptr - 1);
                            break;
                        }
                        case 4: {
                            uint* ptr = (uint*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) != *(ptr - 1);
                            break;
                        }
                        case 8: {
                            ulong* ptr = (ulong*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) != *(ptr - 1);
                            break;
                        }
                        case 16: {
                            UInt128* ptr = (UInt128*)bytePtr;
                            *(bool*)(ptr - 2) = *(ptr - 2) != *(ptr - 1);
                            break;
                        }
                    }
                }

                StackPtr -= 2 * i.TypeSize - 1;

                Console.WriteLine($"{i.Code} {i.TypeSize}\n    {StackString}\n");
                break;

            // casts

            case OperationCode.itof:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1: {
                            sbyte* ptr = (sbyte*)bytePtr - 1;

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
                            short* ptr = (short*)bytePtr - 1;

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
                            int* ptr = (int*)bytePtr - 1;

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
                            long* ptr = (long*)bytePtr - 1;

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
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.utof:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 1: {
                            byte* ptr = bytePtr - 1;

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
                            ushort* ptr = (ushort*)bytePtr - 1;

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
                            uint* ptr = (uint*)bytePtr - 1;

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
                            ulong* ptr = (ulong*)bytePtr - 1;

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
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftof:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr - 1;

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
                            float* ptr = (float*)bytePtr - 1;

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
                            double* ptr = (double*)bytePtr - 1;

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
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftoi:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr - 1;

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
                            float* ptr = (float*)bytePtr - 1;

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
                            double* ptr = (double*)bytePtr - 1;

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
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            case OperationCode.ftou:
                fixed (byte* bytePtr = &Stack[StackPtr]) {
                    switch (i.TypeSize) {
                        case 2: {
                            Half* ptr = (Half*)bytePtr - 1;

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
                            float* ptr = (float*)bytePtr - 1;

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
                            double* ptr = (double*)bytePtr - 1;

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
                }

                StackPtr += i.SecondTypeSize - i.TypeSize;

                Console.WriteLine($"{i.Code} {i.TypeSize} {i.SecondTypeSize}\n    {StackString}\n");
                break;

            default:
                throw new ArgumentException($"Interpreter could not execute instruction '{i.Code}'");
        }

        // move to the next instruction
        InstructionPtr++;
        goto start;
    }

    /// <summary>
    /// Debug property: return the current state of the stack.
    /// </summary>
    private string StackString {
        get { return $"stack: {string.Join(" ", Stack[..StackPtr].ToArray().Select(x => $"{x:X2}"))}"; }
    }
}