using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

public sealed class ImmutableListPool<THandle, TElement> : IPool<THandle, TElement>
    where THandle : IHandle<THandle>
    where TElement : notnull
{
    private readonly List<TElement> list;

    public ImmutableListPool(List<TElement> list)
    {
        this.list = list;
    }
    
    public ImmutableListPool(IEnumerable<TElement> list)
    {
        this.list = list.ToList();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TElement this[THandle handle] => list[handle.Value];

    public THandle AddItem(TElement item)
    {
        throw new InvalidOperationException("This pool is immutable");
    }
}