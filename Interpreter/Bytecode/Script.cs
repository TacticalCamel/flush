namespace Interpreter.Bytecode;

/// <summary>
/// Represents a compiled program.
/// </summary>
public sealed unsafe class Script {
    /// <summary>
    /// The header of the program.
    /// </summary>
    public readonly FileHeader Header;
    
    /// <summary>
    /// The data section of the program.
    /// </summary>
    public readonly ReadOnlyMemory<byte> Data;
    
    /// <summary>
    /// The instructions of the program.
    /// </summary>
    public readonly ReadOnlyMemory<Instruction> Instructions;

    /// <summary>
    /// Creates a new program by assigning the field values directly.
    /// </summary>
    /// <param name="header">The header of the program.</param>
    /// <param name="data">The data section of the program.</param>
    /// <param name="instructions">The instructions of the program.</param>
    internal Script(FileHeader header, ReadOnlyMemory<byte> data, ReadOnlyMemory<Instruction> instructions) {
        Header = header;
        Data = data;
        Instructions = instructions;
    }

    /// <summary>
    /// Creates a new program from a data and instruction array.
    /// </summary>
    /// <param name="data">The data array.</param>
    /// <param name="instructions">The instruction array.</param>
    public Script(byte[] data, Instruction[] instructions) {
        // create a new header
        Header = new FileHeader {
            Signature = FileHeader.VALID_SIGNATURE,
            Version = BinarySerializer.VersionToU64(ClassLoader.BytecodeVersion),
            CompilationTime = DateTime.Now,
            DataStart = sizeof(FileHeader),
            CodeStart = sizeof(FileHeader) + Data.Length
        };

        // assign array fields
        Data = new ReadOnlyMemory<byte>(data);
        Instructions = new ReadOnlyMemory<Instruction>(instructions);
    }

    /// <summary>
    /// Writes the plain text representation of the script to a text writer.
    /// </summary>
    /// <param name="stream">The text writer to write to.</param>
    public void WriteStringContents(TextWriter stream) {
        // header
        stream.WriteLine(".meta");
        stream.WriteLine($"    header           0x{Header.Signature:X16}");
        stream.WriteLine($"    version          {BinarySerializer.U64ToVersion(Header.Version)}");
        stream.WriteLine($"    compiled         {Header.CompilationTime:O}");
        stream.WriteLine($"    data-offset      0x{Header.DataStart:X8}");
        stream.WriteLine($"    code-offset      0x{Header.CodeStart:X8}");
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
                stream.Write($"{(j % 2 == 0 ? ' ' : string.Empty)}{instruction.Bytes[j]:X2}");
            }

            stream.WriteLine();
        }
    }
}