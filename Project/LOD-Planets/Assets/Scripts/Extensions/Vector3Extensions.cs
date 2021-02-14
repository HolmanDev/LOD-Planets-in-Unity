using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// Change a direction's frame of reference from localspace oldcontext to localspace new context.
    /// </summary>
    public static Vector3 RecontextualizeDirection(this Vector3 direction, Transform oldContext, Transform newContext)
    {
        return newContext.InverseTransformDirection(oldContext.TransformDirection(direction));
    }

    /// <summary>
    /// Change a point's frame of reference.
    /// </summary>
    public static Vector3 RecontextualizePoint(this Vector3 point, Transform oldContext, Transform newContext)
    {
        return newContext.InverseTransformDirection(oldContext.position + oldContext.TransformDirection(point) - newContext.position);
    }

    // Rotate degrees around axis. In most cases, all vectors should be in the same reference frame.
    public static Vector3 Rotate(this Vector3 direction, float degrees, Vector3 axis) {
        Quaternion rotation = Quaternion.Euler(axis * degrees);
        return rotation * direction;
    }

    /// <summary>
    /// Project a vector onto a plane, removing all magnitude in the direction of the normal.
    /// </summary>
    public static Vector3 Project(this ref Vector3 vector, Vector3 normal) {
        normal.Normalize();
        vector -= Vector3.Dot(vector, normal) * normal;
        return vector;
    }

    public static Vector3 spherified(this Vector3 vector, double radius) {
        double magnitude = radius / (double) vector.magnitude;
        return vector * (float) magnitude;
    }

    public static Vector2 RoundToWorldAxis(this ref Vector3 vector) {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        float highestValue = 0;
        int closestIndex = 0;
        for(int i = 0; i < directions.Length; i++) {
            float value = Vector3.Dot(vector, directions[i]);
            if(value > highestValue) {
                highestValue = value;
                closestIndex = i;
            }
        }
        return directions[closestIndex];
    }
}
