namespace Interpreter.Bytecode;

using System.Text;

public sealed class Script {
    public MetaData Meta { get; }
    public ReadOnlyMemory<byte> Data { get; }
    public ReadOnlyMemory<Instruction> Instructions { get; }

    public Script(byte[] data, Instruction[] instructions) {
        Data = new ReadOnlyMemory<byte>(data);
        Instructions = new ReadOnlyMemory<Instruction>(instructions);

        int size = Marshal.SizeOf<MetaData>();
        
        Meta = new MetaData {
            Version = EncodeVersion(ScriptExecutor.BytecodeVersion),
            CompilationTime = DateTime.Now,
            DataOffset = size,
            CodeOffset = size + Data.Length
        };
    }

    private Script(MetaData meta, ReadOnlyMemory<byte> data, ReadOnlyMemory<Instruction> instructions) {
        Meta = meta;
        Data = data;
        Instructions = instructions;
    }

    public static Script? CreateFromBytes(byte[] bytes, ILogger logger) {
        bool success = MemoryMarshal.TryRead(bytes, out MetaData meta);

        // header is invalid
        if (!success || !meta.IsHeaderValid) {
            logger.BytecodeCorrupted();
            return null;
        }

        // offset values are invalid
        if (meta.DataOffset > meta.CodeOffset || meta.DataOffset > bytes.Length || meta.CodeOffset > bytes.Length) {
            logger.BytecodeCorrupted();
            return null;
        }

        Version current = ScriptExecutor.BytecodeVersion;
        Version target = DecodeVersion(meta.Version);
        
        // bytecode version is different
        if (current != target) {
            logger.BytecodeVersionMismatch(target, current);
            return null;
        }

        ReadOnlyMemory<byte> dataBytes = new(bytes, meta.DataOffset, meta.CodeOffset - meta.DataOffset);
        ReadOnlyMemory<byte> instructionBytes = new(bytes, meta.CodeOffset, bytes.Length - meta.CodeOffset);

        // TODO cast bytes to instructions
        ReadOnlyMemory<Instruction> instructions = new();
        
        return new Script(meta, dataBytes, instructions);
    }
    
    public byte[] ToBytes() {
        ReadOnlySpan<byte> codeBytes = MemoryMarshal.AsBytes(Instructions.Span);
        
        // calculate sizes in bytes
        int metaSize = Marshal.SizeOf<MetaData>();
        int dataSize = Data.Length;
        int codeSize = codeBytes.Length;
        
        // create byte array with the right length
        byte[] bytes = new byte[metaSize + dataSize + codeSize];
        
        // copy properties to the byte array
        MemoryMarshal.Write(bytes.AsSpan(), Meta);

        Data.CopyTo(bytes.AsMemory()[metaSize..]);

        codeBytes.CopyTo(bytes.AsSpan()[(metaSize + dataSize)..]);
        
        return bytes;
    }
    
    private static ulong EncodeVersion(Version v) {
        return ((ulong)v.Major << 48) + ((ulong)v.Minor << 32) + ((ulong)v.Build << 16) + (ulong)v.Revision;
    }

    private static Version DecodeVersion(ulong v) {
        return new Version((ushort)(v >> 48), (ushort)(v >> 32), (ushort)(v >> 16), (ushort)v);
    }

    public override string ToString() {
        StringBuilder sb = new();

        sb.AppendLine(".meta");
        sb.AppendLine($"    header           0x{Meta.Header:X8}");
        sb.AppendLine($"    version          {DecodeVersion(Meta.Version)}");
        sb.AppendLine($"    compiled         {Meta.CompilationTime.ToString("O")}");
        sb.AppendLine($"    data-offset      0x{Meta.DataOffset:X8}");
        sb.AppendLine($"    code-offset      0x{Meta.CodeOffset:X8}");
        
        sb.AppendLine("\n.data");
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
        
        sb.AppendLine("\n.code");
        foreach (Instruction instruction in Instructions.Span) {
            string? name = Enum.GetName(instruction.OperationCode);

            if (name is null) {
                sb.AppendLine($"    {instruction.OperationCode:X} {instruction.LeftOperand:X8}");
            }
            else {
                sb.AppendLine($"    {name} {instruction.LeftOperand:X8}");
            }
        }

        return sb.ToString();
    }
}