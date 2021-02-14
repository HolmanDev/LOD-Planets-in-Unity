using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericExtensions
{
    ///<summary>
    /// Populate an entire array with one value
    ///</summary>
    public static T[] Populate<T> (this T[] arr, T value) {
        for(int i = 0; i < arr.Length; i++) {
            arr[i] = value;
        }
        return arr;
    }
}
