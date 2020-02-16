using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Presets : MonoBehaviour
{
    public static int quadRes = 24; // The resolution of the quads
    public static Vector3[][] quadTemplateVertices = new Vector3[16][];
    public static Vector3[][] quadTemplateBorderVertices = new Vector3[16][];
    public static int[][] quadTemplateTriangles = new int[16][];
    public static int[][] quadTemplateBorderTriangles = new int[16][];
    public static int[][] quadTemplateEdgeIndices = new int[16][]; // Keeps track of which indices are on the edge fans

    private void Awake()
    {
        GenerateQuadTemplate(quadRes);
    }

    public void GenerateQuadTemplate(int res)
    {
        Vector3[] selectedQuadTemplateVertices = new Vector3[] { };
        Vector3[] selectedQuadTemplateBorderVertices = new Vector3[] { }; // These are go from -1 and down and are only used for normal calculations
        int[] selectedQuadTemplateTriangles = new int[] { };
        int[] selectedQuadTemplateBorderTriangles = new int[] { }; // Only used to calculate the normals
        int[] selectedQuadTemplateEdgeIndices = new int[] { }; // 0 or 1 depending on if a vertex is on the edge of an edgefan or not

        for (int quadI = 0; quadI < 16; quadI++) {
            selectedQuadTemplateVertices = new Vector3[(res + 1) * (res + 1)];
            selectedQuadTemplateBorderVertices = new Vector3[res * 4 + 8];
            selectedQuadTemplateTriangles = new int[res * res * 6];
            selectedQuadTemplateBorderTriangles = new int[res * 24 + 24];
            selectedQuadTemplateEdgeIndices = new int[(res + 1) * (res + 1)];

            int borderVertexOffset = 0;

            // Vertices
            for (int y = 0; y < (res + 1); y++)
            {
                for (int x = 0; x < (res + 1); x++)
                {
                    Vector3 pos = new Vector3(x - res / 2f, y - res / 2f, 0) / (quadRes / 2);

                    // Border vertices
                    selectedQuadTemplateVertices[y * (res + 1) + x] = pos;
                    if (x == 0 && y == 0)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3(-(2f + 2 * (quadI & 2) * 0.5f) / quadRes, -(2f + 2 * (quadI & 4) * 0.25f) / quadRes, 0);
                        selectedQuadTemplateBorderVertices[borderVertexOffset + res + 3] = pos + new Vector3(-(2f + 2 * (quadI & 2) * 0.5f) / quadRes, 0, 0);
                        borderVertexOffset++;
                    }

                    if (y == 0)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3(0, -(2f + 2 * (quadI & 4) * 0.25f) / quadRes, 0);
                        borderVertexOffset++;
                    }

                    if (x == res && y == 0)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3((2f + 2 * (quadI & 1)) / quadRes, -(2f + 2 * (quadI & 4) * 0.25f) / quadRes, 0);
                        borderVertexOffset+=2;
                    }

                    if (x == 0 && y != 0)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3(-(2f + 2 * (quadI & 2) * 0.5f) / quadRes, 0, 0);
                        borderVertexOffset++;
                    }

                    if (x == res && y != res)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3((2f + 2 * (quadI & 1)) / quadRes, 0, 0);
                        borderVertexOffset++;
                    }

                    if (x == 0 && y == res)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset + 1] = pos + new Vector3(-(2f + 2 * (quadI & 2) * 0.5f) / quadRes, (2f + 2 * (quadI & 8) * 0.125f) / quadRes, 0);
                        borderVertexOffset+=2;
                    }

                    if (y == res)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3(0, (2f + 2 * (quadI & 8) * 0.125f) / quadRes, 0);
                        borderVertexOffset++;
                    }

                    if (x == res && y == res)
                    {
                        selectedQuadTemplateBorderVertices[borderVertexOffset] = pos + new Vector3((2f + 2 * (quadI & 1)) / quadRes, (2f + 2 * (quadI & 8) * 0.125f) / quadRes, 0);
                        selectedQuadTemplateBorderVertices[borderVertexOffset - res - 3] = pos + new Vector3((2f + 2 * (quadI & 1)) / quadRes, 0, 0);
                        borderVertexOffset++;
                    }
                }
            }

            int offset = 0;
            int borderOffset = 0;

            // Edges
            for (int i = 0; i < res / 2; i++)
            {
                // Top
                if (Array.Exists(new int[8] { 4, 5, 6, 7, 12, 13, 14, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = i * 2 + res + 2;

                    // Border. The edgefans have both inner and outer borders to accomodate for the fact that they border less detailed quads.
                    if (i % 2 == 0)
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = -i * 2 - 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = -i * 2 - 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = (res + i) * 2 + 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = (res + i) * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = (res + i) * 2 + 4;


                    } else
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = -i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = -i * 2 - 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = (res + i) * 2 + 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = (res + i) * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = (res + i) * 2 + 2;
                    }

                    // Store edge indices
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset]] = 1;
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset + 1]] = 1;

                    offset += 3;
                    borderOffset += 12;
                } else
                {
                    selectedQuadTemplateTriangles[offset] = i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = i * 2 + res + 2;

                    selectedQuadTemplateTriangles[offset + 3] = i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 4] = i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 5] = i * 2 + res + 2;

                    // Border
                    selectedQuadTemplateBorderTriangles[borderOffset] = -i * 2 - 3;
                    selectedQuadTemplateBorderTriangles[borderOffset + 1] = i * 2 + 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 2] = i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 3] = -i * 2 - 3;
                    selectedQuadTemplateBorderTriangles[borderOffset + 4] = i * 2 + 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 5] = i * 2 + 1;

                    selectedQuadTemplateBorderTriangles[borderOffset + 6] = -i * 2 - 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 7] = -i * 2 - 3;
                    selectedQuadTemplateBorderTriangles[borderOffset + 8] = i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 9] = -i * 2 - 3;
                    selectedQuadTemplateBorderTriangles[borderOffset + 10] = -i * 2 - 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 11] = i * 2 + 2;

                    offset += 6;
                    borderOffset += 12;
                }

                // Bottom
                if (Array.Exists(new int[8] { 8, 9, 10, 11, 12, 13, 14, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 1] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = (res + 1) * (res + 1) - res + i * 2 - 1;

                    // Border
                    if (i % 2 == 0)
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = (res + 1) * (res + 1) - res + i * 2 + 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = -res * 3 - 9 - i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = -res * 3 - 9 - i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = -res * 3 - 7 - i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = (res + 1) * (res - 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = (res + 1) * (res - 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = (res + 1) * (res - 2) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    } else
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = (res + 1) * (res + 1) - res + i * 2 + 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = -res * 3 - 7 - i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = (res + 1) * (res + 1) - res + i * 2 + 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = -res * 3 - 9 - i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = -res * 3 - 7 - i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = (res + 1) * (res + 1) - res + i * 2 + 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = (res + 1) * (res - 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = (res + 1) * (res - 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = (res + 1) * (res + 1) - res + i * 2 - 1;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = (res + 1) * (res - 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    }

                    // Store edge indices
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset + 1]] = 1;
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset + 2]] = 1;

                    offset += 3;
                    borderOffset += 12;
                } else
                {
                    selectedQuadTemplateTriangles[offset] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 1] = (res + 1) * (res + 1) - res + i * 2;
                    selectedQuadTemplateTriangles[offset + 2] = (res + 1) * (res + 1) - res + i * 2 - 1;

                    selectedQuadTemplateTriangles[offset + 3] = (res + 1) * (res + 1) - res * 2 + i * 2 - 1;
                    selectedQuadTemplateTriangles[offset + 4] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 5] = (res + 1) * (res + 1) - res + i * 2;

                    // Border
                    selectedQuadTemplateBorderTriangles[borderOffset] = (res + 1) * (res + 1) - res + i * 2 - 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 1] = (res + 1) * (res + 1) - res + i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 2] = -res * 3 - 8 - i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 3] = (res + 1) * (res + 1) - res + i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 4] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 5] = -res * 3 - 8 - i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 6] = -res * 3 - 7 - i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 7] = (res + 1) * (res + 1) - res + i * 2 - 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 8] = -res * 3 - 8 - i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 9] = -res * 3 - 8 - i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 10] = (res + 1) * (res + 1) - res + i * 2 + 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 11] = -res * 3 - 9 - i * 2;

                    offset += 6;
                    borderOffset += 12;
                }

                // Right
                if (Array.Exists(new int[8] { 1, 3, 5, 7, 9, 11, 13, 15}, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2 + 1) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 3) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2;

                    // Border
                    if (i % 2 == 0)
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = res * (i * 2 + 1) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = -res - 5 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = -res - 9 - i * 4;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -res - 9 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = res * (i * 2 + 3) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = res * (i * 2 + 1) + i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = res * (i * 2 + 3) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = res * (i * 2 + 1) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = res * (i * 2 + 3) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = res * (i * 2 + 1) + i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = res * (i * 2 + 1) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = res * (i * 2 + 3) + i * 2;
                    } else
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = res * (i * 2 + 3) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = -res - 5 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = -res - 9 - i * 4;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -res - 5 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = res * (i * 2 + 3) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = res * (i * 2 + 1) + i * 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = res * (i * 2 + 1) + i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = res * (i * 2 + 1) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = res * (i * 2 + 3) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = res * (i * 2 + 1) + i * 2 - 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = res * (i * 2 + 3) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = res * (i * 2 + 3) + i * 2;
                    }

                    // Store edge indices
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset]] = 1;
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset + 1]] = 1;

                    offset += 3;
                    borderOffset += 12;
                }
                else
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2 + 1) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 2) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2;

                    selectedQuadTemplateTriangles[offset + 3] = res * (i * 2 + 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 4] = res * (i * 2 + 2) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 5] = res * (i * 2 + 3) + i * 2 + 2;

                    // Border
                    selectedQuadTemplateBorderTriangles[borderOffset] = res * (i * 2 + 1) + i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 1] = -res - 5 - i * 4 - 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 2] = res * (i * 2 + 2) + i * 2 + 1;

                    selectedQuadTemplateBorderTriangles[borderOffset + 3] = res * (i * 2 + 2) + i * 2 + 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 4] = -res - 7 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 5] = res * (i * 2 + 3) + i * 2 + 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 6] = -res - 5 - i * 4; 
                    selectedQuadTemplateBorderTriangles[borderOffset + 7] = -res - 7 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 8] = res * (i * 2 + 1) + i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 9] = res * (i * 2 + 3) + i * 2 + 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 10] = -res - 7 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 11] = -res - 9 - i * 4;

                    offset += 6;
                    borderOffset += 12;
                }

                // Left
                if (Array.Exists(new int[8] { 2, 3, 6, 7, 10, 11, 14, 15 }, p => p == quadI))
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 2) + i * 2 + 2;

                    // Border
                    if (i % 2 == 0)
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = -res - 8 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = res * (i * 2 + 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -res - 4 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = -res - 8 - i * 4;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = res * (i * 2 + 2) + i * 2 + 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = res * (i * 2 + 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = res * (i * 2) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = res * (i * 2 + 2) + i * 2 + 4;
                    } else
                    {
                        selectedQuadTemplateBorderTriangles[borderOffset] = -res - 4 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 1] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 2] = res * (i * 2 + 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 3] = -res - 4 - i * 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 4] = res * (i * 2 + 2) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 5] = -res - 8 - i * 4;

                        selectedQuadTemplateBorderTriangles[borderOffset + 6] = res * (i * 2) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 7] = res * (i * 2 + 2) + i * 2 + 4;
                        selectedQuadTemplateBorderTriangles[borderOffset + 8] = res * (i * 2 + 2) + i * 2 + 2;

                        selectedQuadTemplateBorderTriangles[borderOffset + 9] = res * (i * 2) + i * 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 10] = res * (i * 2) + i * 2 + 2;
                        selectedQuadTemplateBorderTriangles[borderOffset + 11] = res * (i * 2 + 2) + i * 2 + 2;
                    }

                    // Store edge indices
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset]] = 1;
                    selectedQuadTemplateEdgeIndices[selectedQuadTemplateTriangles[offset + 2]] = 1;

                    offset += 3;
                    borderOffset += 12;
                }
                else
                {
                    selectedQuadTemplateTriangles[offset] = res * (i * 2) + i * 2;
                    selectedQuadTemplateTriangles[offset + 1] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 2] = res * (i * 2 + 1) + i * 2 + 1;

                    selectedQuadTemplateTriangles[offset + 3] = res * (i * 2 + 1) + i * 2 + 1;
                    selectedQuadTemplateTriangles[offset + 4] = res * (i * 2 + 1) + i * 2 + 2;
                    selectedQuadTemplateTriangles[offset + 5] = res * (i * 2 + 2) + i * 2 + 2;

                    // Border
                    selectedQuadTemplateBorderTriangles[borderOffset] =  res * (i * 2 + 1) + i * 2 + 1;
                    selectedQuadTemplateBorderTriangles[borderOffset + 1] = -res - 6 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 2] = res * (i * 2) + i * 2;

                    selectedQuadTemplateBorderTriangles[borderOffset + 3] = res * (i * 2 + 2) + i * 2 + 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 4] = -res - 6 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 5] = res * (i * 2 + 1) + i * 2 + 1;

                    selectedQuadTemplateBorderTriangles[borderOffset + 6] = -res - 4 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 7] = res * (i * 2) + i * 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 8] = -res - 6 - i * 4;

                    selectedQuadTemplateBorderTriangles[borderOffset + 9] = -res - 6 - i * 4;
                    selectedQuadTemplateBorderTriangles[borderOffset + 10] = res * (i * 2 + 2) + i * 2 + 2;
                    selectedQuadTemplateBorderTriangles[borderOffset + 11] =  -res - 8 - i * 4;

                    offset += 6;
                    borderOffset += 12;
                }
            }

            // Transition
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

            // Apply everything
            quadTemplateVertices[quadI] = selectedQuadTemplateVertices;
            quadTemplateBorderVertices[quadI] = selectedQuadTemplateBorderVertices;
            quadTemplateTriangles[quadI] = selectedQuadTemplateTriangles;
            quadTemplateBorderTriangles[quadI] = selectedQuadTemplateBorderTriangles;
            quadTemplateEdgeIndices[quadI] = selectedQuadTemplateEdgeIndices;
        }
    }
}
