using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ListVariable<T> : Variable<List<T>>
{
    public Action<T> onListValueChange;

    public void Add(T thing)
    {
        if (!value.Contains(thing))
        {
            value.Add(thing);
            onListValueChange?.Invoke(thing);
        }
        value.RemoveAll(v => v == null);
    }

    public void Remove(T thing)
    {
        if (value.Contains(thing))
        {
            value.Remove(thing);
            onListValueChange?.Invoke(thing);
        }
        value.RemoveAll(v => v == null);
    }

    public void Clear()
    {
        foreach (var item in value)
        {
            Remove(item);
        }
        onValueChange.Invoke(value);
    }
}