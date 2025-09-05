namespace Tomat.Hacksaw.Metadata;

public sealed class RawTable
{
    public TableKind Kind { get; set; } = TableKind.Raw;

    public byte[] Data { get; set; } = [];
}