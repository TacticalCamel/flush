namespace Interpreter;

using Bytecode;
using System.Buffers;

/// <summary>
/// Class with methods that help conversion between some managed types and their binary representation.
/// </summary>
public static class BinarySerializer {
    /// <summary>
    /// Get the current version of the interpreter assembly.
    /// </summary>
    public static Version BytecodeVersion => typeof(BinarySerializer).Assembly.GetName().Version ?? new Version();

    /// <summary>
    /// Convert a version to an unsigned 64-bit integer.
    /// </summary>
    /// <remarks>
    /// Using 16 bits for each field is enough, since assembly versions are limited to this value.
    /// </remarks>
    /// <param name="version">The version to convert.</param>
    /// <returns>The value representing the version.</returns>
    public static ulong VersionToU64(Version version) {
        return ((ulong)version.Major << 48) + ((ulong)version.Minor << 32) + ((ulong)version.Build << 16) + (ulong)version.Revision;
    }

    /// <summary>
    /// Convert an unsigned 64-bit integer to a version.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The created version.</returns>
    public static Version U64ToVersion(ulong value) {
        return new Version((ushort)(value >> 48), (ushort)(value >> 32), (ushort)(value >> 16), (ushort)value);
    }

    /// <summary>
    /// Convert a script to an array of bytes.
    /// </summary>
    /// <param name="script">The script to convert.</param>
    /// <returns>The byte array.</returns>
    public static byte[] ScriptToBytes(Script script) {
        // instructions as bytes
        ReadOnlySpan<byte> codeBytes = MemoryMarshal.AsBytes(script.Instructions.Span);

        // calculate sizes in bytes
        int metaSize = Marshal.SizeOf<FileHeader>();
        int dataSize = script.Data.Length;
        int codeSize = codeBytes.Length;

        // create byte array with the right length
        byte[] bytes = new byte[metaSize + dataSize + codeSize];

        // copy meta
        MemoryMarshal.Write(bytes.AsSpan(), script.Header);

        // copy data
        script.Data.CopyTo(bytes.AsMemory()[metaSize..]);

        // copy instructions
        codeBytes.CopyTo(bytes.AsSpan()[(metaSize + dataSize)..]);

        return bytes;
    }

    /// <summary>
    /// Convert an array of bytes to a script.
    /// </summary>
    /// <param name="bytes">The bytes to convert.</param>
    /// <param name="logger">A logger to log conversion errors.</param>
    /// <returns>The script if successful, null otherwise.</returns>
    public static Script? BytesToScript(byte[] bytes, ILogger logger) {
        // header is invalid
        if (!MemoryMarshal.TryRead(bytes, out FileHeader meta) || meta.Signature != FileHeader.VALID_SIGNATURE) {
            logger.BytecodeCorrupted();
            return null;
        }

        // offset values are invalid
        if (meta.DataStart > meta.CodeStart || meta.DataStart > bytes.Length || meta.CodeStart > bytes.Length) {
            logger.BytecodeCorrupted();
            return null;
        }

        Version targetVersion = U64ToVersion(meta.Version);

        // bytecode version is different
        if (BytecodeVersion != targetVersion) {
            logger.BytecodeVersionMismatch(targetVersion, BytecodeVersion);
            return null;
        }

        // get data
        ReadOnlyMemory<byte> data = new(bytes, meta.DataStart, meta.CodeStart - meta.DataStart);

        // get instructions
        Memory<byte> instructionBytes = new(bytes, meta.CodeStart, bytes.Length - meta.CodeStart);
        ReadOnlyMemory<Instruction> instructions = CastMemoryManager<byte, Instruction>.Cast(instructionBytes);

        return new Script(meta, data, instructions);
    }

    /// <summary>
    /// This class helps casting unmanaged memory from one type to another.
    /// </summary>
    /// <typeparam name="TFrom">The unmanaged type to cast from.</typeparam>
    /// <typeparam name="TTo">The unmanaged type to cast to.</typeparam>
    private sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged {
        /// <summary>
        /// The memory to cast.
        /// </summary>
        private readonly Memory<TFrom> From;

        /// <summary>
        /// Create a new memory manager.
        /// </summary>
        /// <param name="from">The memory to cast.</param>
        /// <typeparam name="TFrom">The unmanaged type to cast from.</typeparam>
        /// <typeparam name="TTo">The unmanaged type to cast to.</typeparam>
        private CastMemoryManager(Memory<TFrom> from) {
            From = from;
        }

        /// <summary>
        /// Cast unmanaged memory from one type to another.
        /// </summary>
        /// <param name="memory">The memory to cast.</param>
        /// <returns>The same memory cast to the target type.</returns>
        public static Memory<TTo> Cast(Memory<TFrom> memory) {
            return new CastMemoryManager<TFrom, TTo>(memory).Memory;
        }

        public override Span<TTo> GetSpan() {
            return MemoryMarshal.Cast<TFrom, TTo>(From.Span);
        }

        protected override void Dispose(bool disposing) { }

        public override MemoryHandle Pin(int elementIndex = 0) {
            throw new NotSupportedException();
        }

        public override void Unpin() {
            throw new NotSupportedException();
        }
    }
}