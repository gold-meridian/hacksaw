using System.Collections.Generic;

namespace Tomat.Hacksaw.Metadata.Image.Pooling;

/// <summary>
///     An arbitrary item pool in an <see cref="HlImage"/>.
/// </summary>
/// <typeparam name="THandle">The handle this pool provies.</typeparam>
/// <typeparam name="TElement">The type of the elements in the pool.</typeparam>
public interface IPool<THandle, TElement> : IEnumerable<TElement>
    where THandle : IHandle<THandle>
    where TElement : notnull
{
    /// <summary>
    ///     Accesses an item from the pool given a handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>
    ///     The value associated with the handle, or <see langword="null"/> if
    ///     the handle is unknown.
    /// </returns>
    TElement? this[THandle handle] { get; }

    /// <summary>
    ///     Adds an item to the pool.  If this item is already present to the
    ///     pool no changes are made.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>
    ///     A handle pointing to the added item, to be used elsewhere.
    /// </returns>
    THandle AddItem(TElement item);
}