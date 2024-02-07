namespace Interpreter.Serialization;

using Bytecode;

public static class BinarySerializer {
    public static ulong VersionToU64(Version version) {
        return ((ulong)version.Major << 48) + ((ulong)version.Minor << 32) + ((ulong)version.Build << 16) + (ulong)version.Revision;
    }

    public static Version U64ToVersion(ulong value) {
        return new Version((ushort)(value >> 48), (ushort)(value >> 32), (ushort)(value >> 16), (ushort)value);
    }
    
    public static byte[] ScriptToBytes(Script script) {
        ReadOnlySpan<byte> codeBytes = MemoryMarshal.AsBytes(script.Instructions.Span);

        // calculate sizes in bytes
        int metaSize = Marshal.SizeOf<MetaData>();
        int dataSize = script.Data.Length;
        int codeSize = codeBytes.Length;

        // create byte array with the right length
        byte[] bytes = new byte[metaSize + dataSize + codeSize];

        // copy properties to the byte array
        MemoryMarshal.Write(bytes.AsSpan(), script.Meta);

        script.Data.CopyTo(bytes.AsMemory()[metaSize..]);

        codeBytes.CopyTo(bytes.AsSpan()[(metaSize + dataSize)..]);

        return bytes;
    }
    
    public static Script? BytesToScript(byte[] bytes, ILogger logger) {
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

        Version currentVersion = ScriptExecutor.BytecodeVersion;
        Version targetVersion = U64ToVersion(meta.Version);

        // bytecode version is different
        if (currentVersion != targetVersion) {
            logger.BytecodeVersionMismatch(targetVersion, currentVersion);
            return null;
        }

        // get data
        ReadOnlyMemory<byte> data = new(bytes, meta.DataOffset, meta.CodeOffset - meta.DataOffset);

        // get instructions
        Memory<byte> instructionBytes = new(bytes, meta.CodeOffset, bytes.Length - meta.CodeOffset);
        ReadOnlyMemory<Instruction> instructions = CastMemoryManager<byte, Instruction>.Cast(instructionBytes);
        
        return new Script(meta, data, instructions);
    }
}