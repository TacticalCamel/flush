namespace Interpreter.Serialization;

using Types;
using System.Text;
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
        StringBuilder sb = new();

        // meta section
        sb.AppendLine(".meta");
        sb.AppendLine($"    header           0x{Meta.Header:X16}");
        sb.AppendLine($"    version          {BinarySerializer.U64ToVersion(Meta.Version)}");
        sb.AppendLine($"    compiled         {Meta.CompilationTime.ToString("O")}");
        sb.AppendLine($"    data-offset      0x{Meta.DataOffset:X8}");
        sb.AppendLine($"    code-offset      0x{Meta.CodeOffset:X8}");
        sb.AppendLine();

        // data section
        sb.AppendLine(".data");
        
        for (int i = 0; i < Data.Length; i += 16) {
            int end = Math.Min(i + 16, Data.Length);

            ReadOnlySpan<byte> row = Data.Span[i..end];

            sb.Append($"    0x{i:X8}       ");
            
            foreach (byte b in row) {
                sb.Append($"{b:X2} ");
            }

            sb.AppendLine();
        }

        sb.AppendLine();

        // code section
        int instructionSize = Marshal.SizeOf<Instruction>();

        sb.AppendLine(".code");
        for (int i = 0; i < Instructions.Span.Length; i++) {
            Instruction instruction = Instructions.Span[i];
            string name = Enum.GetName(instruction.Code) ?? instruction.Code.ToString("X");

            sb.AppendLine($"    0x{instructionSize * i:X8}       {name} {instruction.DataAddress:X2}");
        }

        return sb.ToString();
    }
}