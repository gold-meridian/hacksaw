using System.Runtime.InteropServices;

namespace Tomat.Hacksaw.Metadata;

/// <summary>
///     Row identifier pointing to a value within a table.
/// </summary>
/// <param name="Kind">The kind of table.</param>
/// <param name="Index">The index of the element within the table.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct RowId(TableKind Kind, uint Index);