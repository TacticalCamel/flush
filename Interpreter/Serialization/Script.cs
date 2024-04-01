namespace Interpreter.Serialization;

using Bytecode;

public sealed class Script {
    public readonly MetaData Meta;
    public readonly ReadOnlyMemory<byte> Data;
    public readonly ReadOnlyMemory<Instruction> Instructions;

    internal Script(MetaData meta, ReadOnlyMemory<byte> data, ReadOnlyMemory<Instruction> instructions) {
        Meta = meta;
        Data = data;
        Instructions = instructions;
    }

    public Script(byte[] data, Instruction[] instructions) {
        int size = Marshal.SizeOf<MetaData>();

        Data = new ReadOnlyMemory<byte>(data);
        Instructions = new ReadOnlyMemory<Instruction>(instructions);
        Meta = new MetaData {
            Version = BinarySerializer.VersionToU64(ClassLoader.BytecodeVersion),
            CompilationTime = DateTime.Now,
            DataOffset = size,
            CodeOffset = size + Data.Length
        };
    }

    public override string ToString() {
        return $"script(data[{Data.Length}], instructions[{Instructions.Length}])";
    }

    public unsafe void WriteStringContentsToBuffer(TextWriter stream) {
        // meta section
        stream.WriteLine(".meta");
        stream.WriteLine($"    header           0x{Meta.Header:X16}");
        stream.WriteLine($"    version          {BinarySerializer.U64ToVersion(Meta.Version)}");
        stream.WriteLine($"    compiled         {Meta.CompilationTime:O}");
        stream.WriteLine($"    data-offset      0x{Meta.DataOffset:X8}");
        stream.WriteLine($"    code-offset      0x{Meta.CodeOffset:X8}");
        stream.WriteLine();

        // data section
        stream.WriteLine(".data");

        for (int i = 0; i < Data.Length; i += 16) {
            int end = Math.Min(i + 16, Data.Length);

            ReadOnlySpan<byte> row = Data.Span[i..end];

            stream.Write($"    0x{i:X8}      ");

            for (int j = 0; j < row.Length; j++) {
                byte b = row[j];
                stream.Write($"{(j % 2 == 0 ? ' ' : string.Empty)}{b:X2}");
            }

            stream.WriteLine();
        }

        stream.WriteLine();

        // code section
        int instructionSize = sizeof(Instruction);

        stream.WriteLine(".code");
        
        for (int i = 0; i < Instructions.Span.Length; i++) {
            Instruction instruction = Instructions.Span[i];
            string name = Enum.GetName(instruction.Code)?.ToLower() ?? instruction.Code.ToString("X");

            stream.Write($"    0x{instructionSize * i:X8}       {name,-4}");

            for (int j = 0; j < instructionSize; j++) {
                stream.Write($"{(j % 2 == 0 ? ' ' : string.Empty)}{instruction.Data[j]:X2}");
            }

            stream.WriteLine();
        }
    }
}