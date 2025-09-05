namespace Tomat.Hacksaw.Metadata;

public readonly record struct TableIndex(
    TableKind Kind,
    uint RowCount,
    long Offset,
    int RowSize
)
{
    public bool IsVariable => RowSize == 0;
}