namespace Tomat.Hacksaw.IO;

public sealed class MemoryByteSource(ReadOnlyMemory<byte> data) : IByteSource
{
    public long Length => data.Length;

    public int ReadAt(long offset, Span<byte> buffer)
    {
        if (offset >= Length)
        {
            return 0;
        }

        var avail = (int)Math.Min(buffer.Length, data.Length - offset);
        {
            data.Slice((int)offset, avail).Span.CopyTo(buffer[..avail]);
        }
        return avail;
    }

    public ValueTask<int> ReadAtAsync(long offset, Memory<byte> buffer, CancellationToken token = default)
    {
        if (offset >= data.Length)
        {
            return new ValueTask<int>(0);
        }
        
        var avail = (int)Math.Min(buffer.Length, data.Length - offset);
        {
            data.Slice((int)offset, avail).CopyTo(buffer[..avail]);
        }

        return new ValueTask<int>(avail);
    }

    public ValueTask DisposeAsync()
    {
        // Nothing to free.
        
        return ValueTask.CompletedTask;
    }
}