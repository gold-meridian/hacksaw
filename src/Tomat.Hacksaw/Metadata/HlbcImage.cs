using System.Collections.ObjectModel;

using Tomat.Hacksaw.IO;

namespace Tomat.Hacksaw.Metadata;

public sealed class HlbcImage(
    IByteSource source,
    byte version,
    uint flags,
    IReadOnlyDictionary<TableKind, TableIndex> tables,
    List<RawTable> rawTables
) : IDisposable
{
    public byte Version { get; } = version;

    public uint Flags { get; } = flags;

    public IReadOnlyDictionary<TableKind, TableIndex> Tables { get; } = tables;

    public List<RawTable> RawTables { get; } = rawTables;

    private readonly IByteSource source = source;

    public void Dispose()
    {
        // TODO release managed resources here
    }
}