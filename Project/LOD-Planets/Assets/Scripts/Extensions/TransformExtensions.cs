using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Returns a child with the specified tag.
    /// </summary>
    public static Transform GetChildWithTag(this Transform transform, string tag)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == tag)
            {
                return transform.GetChild(i);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns all children with the specified tag.
    /// </summary>
    public static Transform[] GetChildrenWithTag(this Transform transform, string tag)
    {
        List<Transform> childrenWithTag = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == tag)
            {
                childrenWithTag.Add(transform.GetChild(i));
            }
        }

        return childrenWithTag.ToArray();
    }
}
