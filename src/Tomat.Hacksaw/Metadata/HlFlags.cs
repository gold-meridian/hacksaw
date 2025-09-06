using System;

namespace Tomat.Hacksaw.Metadata;

[Flags]
public enum HlFlags : uint
{
    Debug = 1 << 0,
}