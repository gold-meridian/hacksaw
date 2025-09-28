using System;

namespace Tomat.Hacksaw;

internal sealed class PooledArrayAllocator<T>(int poolSize)
{
    private T[] pool = new T[poolSize];
    private int index;

    public Memory<T> Allocate(int size)
    {
        if (size > poolSize)
        {
            return new T[size];
        }

        if (index + size > poolSize)
        {
            pool = new T[poolSize];
            index = 0;
        }
        
        var bytes = pool.AsMemory(index, size);
        {
            index += size;
        }

        return bytes;
    }
}