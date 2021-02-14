using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoolExtension 
{
    /// <summary>
    /// Treat an array of bools as a binary sequence where true = 1 and false = 0.
    /// </summary>
    public static int AsBinarySequence(this bool[] array) {
        int result = 0;
        
        for(int i = 0; i < array.Length; i++) {
            result |= array[i] ? IntExtensions.PowersOfTwo[i] : 0;
        }
        return result;
    }

    /// <summary>
    /// Treat an array of bools as a binary sequence where true = 1 and false = 0 up to the n-th element.
    /// </summary>
    public static int AsBinarySequence(this bool[] array, int n) {
        int result = 0;
        
        for(int i = 0; i < n; i++) {
            result |= array[i] ? IntExtensions.PowersOfTwo[i] : 0;
        }
        return result;
    }
}
