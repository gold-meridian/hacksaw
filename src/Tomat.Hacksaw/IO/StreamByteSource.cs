namespace Tomat.Hacksaw.IO;

public sealed class StreamByteSource : IByteSource
{
    public long Length => stream.Length;

    private readonly Stream stream;

    public StreamByteSource(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new NotSupportedException("Stream must be seekable");
        }

        this.stream = stream;
    }

    public int ReadAt(long offset, Span<byte> buffer)
    {
        stream.Seek(offset, SeekOrigin.Begin);

        var total = 0;
        while (total < buffer.Length)
        {
            var read = stream.Read(buffer[total..]);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

    public async ValueTask<int> ReadAtAsync(long offset, Memory<byte> buffer, CancellationToken token = default)
    {
        stream.Seek(offset, SeekOrigin.Begin);

        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer[total..], token).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

    public async ValueTask DisposeAsync()
    {
        await stream.DisposeAsync();
    }
}