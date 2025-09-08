using System.Collections;
using System.Collections.Generic;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

/// <summary>
///     Pools elements based on their assigned index.  Any added element is
///     accepted and granted a unique handle to then be used.  Different from
///     <see cref="HashPool{THandle,TElement}"/> because it allows duplicates
///     and consumers are expected to hold onto their new handle to be
///     distributed elsewhere instead of relying on the ability to get the same
///     handle from an equal element instance.
///     <br />
///     While an implementation detail, the difference is encoded in the
///     semantics of the pool and behavior should be evident to consumers
///     through external behavior.
/// </summary>
/// <typeparam name="THandle">The handle this pool provies.</typeparam>
/// <typeparam name="TElement">The type of the elements in the pool.</typeparam>
public sealed class ListPool<THandle, TElement> : IPool<THandle, TElement>
    where THandle : IHandle<THandle>
    where TElement : notnull
{
    private readonly List<TElement> list = [];
    
    public ListPool(IEnumerable<TElement> elements)
    {
        foreach (var element in elements)
        {
            list.Add(element);
        }
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
        var handle = THandle.DangerouslyCreateHandleForPool(list.Count);
        list.Add(item);
        return handle;
    }
}