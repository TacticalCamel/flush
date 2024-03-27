namespace Interpreter.Serialization;

using System.Buffers;

internal sealed class CastMemoryManager<TFrom, TTo>(Memory<TFrom> from) : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged {
    public override Span<TTo> GetSpan() {
        return MemoryMarshal.Cast<TFrom, TTo>(from.Span);
    }

    public static Memory<TTo> Cast(Memory<TFrom> from) {
        return new CastMemoryManager<TFrom, TTo>(from).Memory;
    }

    protected override void Dispose(bool disposing) { }

    public override MemoryHandle Pin(int elementIndex = 0) {
        throw new NotSupportedException();
    }

    public override void Unpin() {
        throw new NotSupportedException();
    }
}