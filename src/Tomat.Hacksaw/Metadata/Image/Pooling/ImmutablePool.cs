using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

public sealed class ImmutablePool<THandle, TElement>(TElement[] array) : IPool<THandle, TElement>
    where THandle : IHandle<THandle>
    where TElement : notnull
{
    public IEnumerator<TElement> GetEnumerator()
    {
        return array.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TElement this[THandle handle] => array[handle.Value];

    public THandle AddItem(TElement item)
    {
        throw new InvalidOperationException("This pool is immutable");
    }
}