namespace Tomat.Hacksaw.IO;

/// <summary>
///     An arbitrary byte source providing mechanics to read arbitrary data.
/// </summary>
public interface IByteSource : IAsyncDisposable
{
    long Length { get; }

    int ReadAt(long offset, Span<byte> buffer);

    ValueTask<int> ReadAtAsync(long offset, Memory<byte> buffer, CancellationToken token = default);
}