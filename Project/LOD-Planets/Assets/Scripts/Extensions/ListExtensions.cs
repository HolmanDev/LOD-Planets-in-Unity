using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// Remove an item from a list and fill it's space with other items from the list.
    /// </summary>
    public static void RemoveAndFill<T> (this List<T> list, T item)
    {
        int length = list.Count;
        int index = list.IndexOf(item);

        if (index >= 0)
        {
            list.RemoveAt(index);
            for (int i = index + 1; i < list.Count; i++)
            {
                list[i - 1] = list[i];
                list.RemoveAt(i);
            }

            list.Capacity = length - 1;
        }
    }

    ///<summary>
    /// Populate an entire array with one value
    ///</summary>
    public static List<T> Populate<T> (this List<T> list, T value) {
        for(int i = 0; i < list.Count; i++) {
            list[i] = value;
        }
        return list;
    }
}
