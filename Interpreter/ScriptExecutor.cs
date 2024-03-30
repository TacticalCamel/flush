namespace Interpreter;

using Serialization;
using Bytecode;

public sealed unsafe class ScriptExecutor(Script script) {
    private readonly ReadOnlyMemory<byte> Data = script.Data;
    private readonly ReadOnlyMemory<Instruction> Instructions = script.Instructions;
    private readonly byte[] Stack = new byte[1 << 20];
    private int InstructionPtr;
    private int StackPtr;

    public void Run() {
        start: ;
        
        if (InstructionPtr >= Instructions.Length) {
            return;
        }
        
        Instruction i = Instructions.Span[InstructionPtr];

        switch (i.Code) {
            // stack operations

            case OperationCode.pshd:
                ReadOnlySpan<byte> data = Data.Span[i.DataAddress..(i.DataAddress + i.TypeSize)];

                data.CopyTo(Stack.AsSpan(StackPtr));
                StackPtr += i.TypeSize;

                Console.WriteLine($"{i.Code} addr=0x{i.DataAddress:X} size={i.TypeSize}\n    {StackString}\n");
                break;

            case OperationCode.pshz:
                Stack.AsSpan(StackPtr..(StackPtr + i.TypeSize)).Clear();
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

        InstructionPtr++;
        goto start;
    }

    /// <summary>
    /// Debug property.
    /// </summary>
    private string StackString {
        get { return $"Stack: {string.Join(" ", Stack.AsSpan(..StackPtr).ToArray().Select(x => $"{x:X2}"))}"; }
    }
}