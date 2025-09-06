using System.Collections;
using System.Collections.Generic;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

/// <summary>
///     Pools elements based on their hash.
/// </summary>
/// <typeparam name="THandle">The handle this pool provies.</typeparam>
/// <typeparam name="TElement">The type of the elements in the pool.</typeparam>
public sealed class HashPool<THandle, TElement> : IPool<THandle, TElement>
    where THandle : IHandle<THandle>
    where TElement : notnull
{
    private readonly Dictionary<THandle, TElement> handleLookup = [];
    private readonly Dictionary<TElement, THandle> elementLookup = [];

    public HashPool(IEnumerable<TElement> elements)
    {
        var i = 0;

        foreach (var element in elements)
        {
            var handle = THandle.DangerouslyCreateHandleForPool(i++);
            handleLookup.Add(handle, element);
            elementLookup.Add(element, handle);
        }
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return handleLookup.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TElement? this[THandle handle] => handleLookup.GetValueOrDefault(handle);

    public THandle AddItem(TElement item)
    {
        if (elementLookup.TryGetValue(item, out var handle))
        {
            return handle;
        }

        handle = THandle.DangerouslyCreateHandleForPool(elementLookup.Count);
        {
            handleLookup.Add(handle, item);
            elementLookup.Add(item, handle);
        }
        return handle;
    }
}