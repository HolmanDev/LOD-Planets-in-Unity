using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationCurveExtensions
{
    /// <summary>
    /// Get the highest absolute value (y) of the animation curve within a range.
    /// </summary>
    public static float GetExtremeY(this AnimationCurve curve, float min, float max, float interval) 
    {
        float highestValue = 0;
        
        for(float i = min; i < max; i += interval) {
            float value = curve.Evaluate(i);
            if(Mathf.Abs(value) > highestValue) highestValue = value;
        }

        return highestValue;
    }

    /// <summary>
    /// Get the x-value of the highest absolute value (y) of the animation curve within a range.
    /// </summary>
    public static float GetExtremeX(this AnimationCurve curve, float min, float max, float interval) 
    {
        float highestValue = 0;
        float x = 0;
        
        for(float i = min; i < max; i += interval) {
            float value = curve.Evaluate(i);
            if(Mathf.Abs(value) > highestValue) { 
                highestValue = value;
                x = i;
            }
        }

        return x;
    }
}
