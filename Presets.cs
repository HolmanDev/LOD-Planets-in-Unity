using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Presets : MonoBehaviour
{
    public static int quadRes = 4; // The resolution of the quads
    public static Vector3[][] quadTemplateVertices = new Vector3[16][];
    public static int[][] quadTemplateTriangles = new int[16][];

    private void Start()
    {
        GenerateQuadTemplate(quadRes);
    }

    public void GenerateQuadTemplate(int res)
    {
        Vector3[] selectedQuadTemplateVertices = new Vector3[] { };
        int[] selectedQuadTemplateTriangles = new int[] { };

        for (int quadI = 0; quadI < 16; quadI++) {
            selectedQuadTemplateVertices = new Vector3[(res + 1) * (res + 1)];
            selectedQuadTemplateTriangles = new int[res * res * 6];

            // Vertices
            for (int y = 0; y < (res + 1); y++)
            {
                for (int x = 0; x < (res + 1); x++)
                {

                    selectedQuadTemplateVertices[y * (res + 1) + x] = new Vector3(x - res / 2f, y - res / 2f, 0) / (quadRes/2);
                }
            }

            int offset = 0;

            // Edges
            for (int i = 0; i < res / 2; i++)
            {
                // Top
                if (Array.Exists(new int[8] { 4, 5, 6, 7, 12, 13, 14, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = i * 2 + res + 2;

                    offset += 3;
                } else
                {
                    selectedQuadTemplateTriangles[offset] = i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = i * 2 + res + 2;

                    selectedQuadTemplateTriangles[offset + 3] = i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 4] = i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 5] = i * 2 + res + 2;

                    offset += 6;
                }

                // Bottom
                if (Array.Exists(new int[8] { 8, 9, 10, 11, 12, 13, 14, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 1] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = (res + 1) * (res + 1) - res + i * 2 - 1;

                    offset += 3;
                } else
                {
                    selectedQuadTemplateTriangles[offset] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 1] = (res + 1) * (res + 1) - res + i * 2;
                    selectedQuadTemplateTriangles[offset + 2] = (res + 1) * (res + 1) - res + i * 2 - 1;

                    selectedQuadTemplateTriangles[offset + 3] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 4] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 5] = (res + 1) * (res + 1) - res + i * 2;

                    offset += 6;
                }

                // Right
                if (Array.Exists(new int[8] { 1, 3, 5, 7, 9, 11, 13, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2 + 1) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 3) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2;

                    offset += 3;
                }
                else
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2 + 1) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 2) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2;

                    selectedQuadTemplateTriangles[offset + 3] = res * (i * 2 + 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 4] = res * (i * 2 + 2) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 5] = res * (i * 2 + 3) + i * 2 + 2;

                    offset += 6;
                }

                // Left
                if (Array.Exists(new int[8] { 2, 3, 6, 7, 10, 11, 14, 15 }, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2 + 2;

                    offset += 3;
                }
                else
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 1) + i * 2 + 1;

                    selectedQuadTemplateTriangles[offset + 3] = res * (i * 2 + 1) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 4] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 5] = res * (i * 2 + 2) + i * 2 + 2;

                    offset += 6;
                }
            }

            // Border
            for (int i = 0; i < res / 2 - 1; i++)
            {
                // Top 1
                selectedQuadTemplateTriangles[offset] = (i + 1) * 2;
                selectedQuadTemplateTriangles[offset + 1] = res + 3 + i * 2;
                selectedQuadTemplateTriangles[offset + 2] = res + 2 + i * 2;

                // Top 2
                selectedQuadTemplateTriangles[offset + 3] = (i + 1) * 2;
                selectedQuadTemplateTriangles[offset + 4] = res + 4 + i * 2;
                selectedQuadTemplateTriangles[offset + 5] = res + 3 + i * 2;

                // Bottom 1
                selectedQuadTemplateTriangles[offset + 6] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                selectedQuadTemplateTriangles[offset + 7] = (res + 1) * (res + 1) - res * 2 + i * 2;
                selectedQuadTemplateTriangles[offset + 8] = (res + 1) * (res + 1) - res + i * 2 + 1;

                // Bottom 2
                selectedQuadTemplateTriangles[offset + 9] = (res + 1) * (res + 1) - res * 2 + i * 2;
                selectedQuadTemplateTriangles[offset + 10] = (res + 1) * (res + 1) - res * 2 + i * 2 + 1;
                selectedQuadTemplateTriangles[offset + 11] = (res + 1) * (res + 1) - res + i * 2 + 1;

                // Right 1
                selectedQuadTemplateTriangles[offset + 12] = res * (i * 2 + 2) + i * 2;
                selectedQuadTemplateTriangles[offset + 13] = res * (i * 2 + 3) + i * 2 + 2;
                selectedQuadTemplateTriangles[offset + 14] = res * (i * 2 + 3) + i * 2 + 1;

                // Right 2
                selectedQuadTemplateTriangles[offset + 15] = res * (i * 2 + 3) + i * 2 + 1;
                selectedQuadTemplateTriangles[offset + 16] = res * (i * 2 + 3) + i * 2 + 2;
                selectedQuadTemplateTriangles[offset + 17] = res * (i * 2 + 4) + i * 2 + 2;

                // Left 1
                selectedQuadTemplateTriangles[offset + 18] = res * (i * 2 + 1) + i * 2 + 2;
                selectedQuadTemplateTriangles[offset + 19] = res * (i * 2 + 2) + i * 2 + 3;
                selectedQuadTemplateTriangles[offset + 20] = res * (i * 2 + 2) + i * 2 + 2;

                // Left 2
                selectedQuadTemplateTriangles[offset + 21] = res * (i * 2 + 2) + i * 2 + 2;
                selectedQuadTemplateTriangles[offset + 22] = res * (i * 2 + 2) + i * 2 + 3;
                selectedQuadTemplateTriangles[offset + 23] = res * (i * 2 + 3) + i * 2 + 4;

                offset += 24;
            }

            // Middle
            int n = 0;
            int middleOffset = res + 2;
            for (int y = 0; y < (res - 2); y++)
            {
                for (int x = 0; x < (res - 2); x++)
                {
                    n = y * (res - 2) + x;
                    selectedQuadTemplateTriangles[offset] = middleOffset + n + y * 3;
                    selectedQuadTemplateTriangles[offset + 1] = middleOffset + n + 1 + y * 3;
                    selectedQuadTemplateTriangles[offset + 2] = middleOffset + n + res + 1 + y * 3;

                    selectedQuadTemplateTriangles[offset + 3] = middleOffset + n + 1 + y * 3;
                    selectedQuadTemplateTriangles[offset + 4] = middleOffset + n + res + 2 + y * 3;
                    selectedQuadTemplateTriangles[offset + 5] = middleOffset + n + res + 1 + y * 3;
                    offset += 6;
                }
            }

            quadTemplateVertices[quadI] = selectedQuadTemplateVertices;
            quadTemplateTriangles[quadI] = selectedQuadTemplateTriangles;
        }
    }
}
