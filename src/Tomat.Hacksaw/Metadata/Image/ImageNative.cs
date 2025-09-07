using Tomat.Hacksaw.Metadata.Image.Pooling;

namespace Tomat.Hacksaw.Metadata.Image;

public readonly record struct ImageNative(
    StringHandle LibraryName,
    StringHandle FunctionName,
    TypeHandle Type,
    uint NativeIndex
);