namespace Interpreter.Serialization;

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
            Version = BinarySerializer.VersionToU64(ScriptExecutor.BytecodeVersion),
            CompilationTime = DateTime.Now,
            DataOffset = size,
            CodeOffset = size + Data.Length
        };
    }
    
    public override string ToString() {
        StringBuilder sb = new();

        sb.AppendLine(".meta");
        sb.AppendLine($"    header           0x{Meta.Header:X16}");
        sb.AppendLine($"    version          {BinarySerializer.U64ToVersion(Meta.Version)}");
        sb.AppendLine($"    compiled         {Meta.CompilationTime.ToString("O")}");
        sb.AppendLine($"    data-offset      0x{Meta.DataOffset:X8}");
        sb.AppendLine($"    code-offset      0x{Meta.CodeOffset:X8}");

        sb.AppendLine();
        
        sb.AppendLine(".data");
        for (int i = 0; i < Data.Length; i += 16) {
            int end = Math.Min(i + 16, Data.Length);

            ReadOnlySpan<byte> row = Data.Span[i..end];

            sb.Append($"    0x{i:X8}       ");
            for (int j = 0; j < row.Length; j++) {
                byte b = row[j];
                sb.Append($"{b:X2}");
                if (j != row.Length - 1) sb.Append(' ');
            }

            sb.AppendLine();
        }

        sb.AppendLine();

        int instructionSize = Marshal.SizeOf<Instruction>();
        
        sb.AppendLine(".code");
        for (int i = 0; i < Instructions.Span.Length; i++) {
            Instruction instruction = Instructions.Span[i];
            string name = Enum.GetName(instruction.OperationCode) ?? instruction.OperationCode.ToString("X");

            sb.AppendLine($"    0x{instructionSize * i:X8}       {name} {instruction.FirstOperand:X2} {instruction.SecondOperand:X2} {instruction.TargetOperand:X2}");
        }

        return sb.ToString();
    }
}