using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class Toolbox : MonoBehaviour
{
    public static void HighlightVertices(Vector3[] vertices, float size, Vector3 offset, int indexToHighlight, float time, Color color)
    {
        int i = -1;
        foreach (Vector3 vertex in vertices)
        {
            Debug.DrawLine(vertex + offset, vertex + Vector3.up * size + offset, (i == indexToHighlight) ? Color.red : color, time, false);
            Debug.DrawLine(vertex + offset, vertex + Vector3.down * size + offset, (i == indexToHighlight) ? Color.red : color, time, false);
            Debug.DrawLine(vertex + offset, vertex + Vector3.right * size + offset, (i == indexToHighlight) ? Color.red : color, time, false);
            Debug.DrawLine(vertex + offset, vertex + Vector3.left * size + offset, (i == indexToHighlight) ? Color.red : color, time, false);

            i--;
        }
    }

    public static void HighlightQuadTriangles(int[] triangles, Vector3[] vertices, Vector3[] borderVertices, Color color)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3[] points = new Vector3[3];

            points[0] = (triangles[i] >= 0) ? vertices[triangles[i]] : borderVertices[-triangles[i] - 1];
            points[1] = (triangles[i + 1] >= 0) ? vertices[triangles[i + 1]] : borderVertices[-triangles[i + 1] - 1];
            points[2] = (triangles[i + 2] >= 0) ? vertices[triangles[i + 2]] : borderVertices[-triangles[i + 2] - 1];

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[0], color);

            Vector3 sideAB = points[1] - points[0];
            Vector3 sideAC = points[2] - points[0];
            Debug.DrawLine(points[0], Vector3.Cross(sideAB, sideAC).normalized, Color.green);
        }
    }

    public static void HighlightVector3(Vector3 origin, Vector3 dir, float length, Color color, float duration)
    {
        Debug.DrawRay(origin, dir.normalized * length, color, duration, false);
    }

    public static void DrawBounds(Bounds b, float delay=0)
    {
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, Color.blue, delay);
        Debug.DrawLine(p2, p3, Color.red, delay);
        Debug.DrawLine(p3, p4, Color.yellow, delay);
        Debug.DrawLine(p4, p1, Color.magenta, delay);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, Color.blue, delay);
        Debug.DrawLine(p6, p7, Color.red, delay);
        Debug.DrawLine(p7, p8, Color.yellow, delay);
        Debug.DrawLine(p8, p5, Color.magenta, delay);

        // sides
        Debug.DrawLine(p1, p5, Color.white, delay);
        Debug.DrawLine(p2, p6, Color.gray, delay);
        Debug.DrawLine(p3, p7, Color.green, delay);
        Debug.DrawLine(p4, p8, Color.cyan, delay);
    }
}
