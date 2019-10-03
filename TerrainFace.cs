using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;

    // These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    // Constructor
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, float radius)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.radius = radius;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    // Construct a quadtree of chunks (even though the chunks end up 3D, they start out 2D in the quadtree and are later projected onto a sphere)
    public void ConstructTree()
    {
        // Resets the mesh
        vertices.Clear();
        triangles.Clear();

        // Generate chunks
        Chunk parentChunk = new Chunk(null, null, localUp.normalized * Planet.size, radius, 0, localUp, axisA, axisB);
        parentChunk.GenerateChildren();

        // Get chunk mesh data
        int triangleOffset = 0;
        foreach(Chunk child in parentChunk.GetVisibleChildren())
        {
            (Vector3[], int[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;
        }

        // Reset mesh and apply new data
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}

public class Chunk
{
    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;

    // Constructor
    public Chunk(Chunk[] children, Chunk parent, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB)
    {
        this.children = children;
        this.parent = parent; // Not currently used but might be in the future
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
    }

    public void GenerateChildren()
    {
        // If the detail level is under max level and above 0. Max level depends on how many detail levels are defined in planets and needs to be changed manually.
        if (detailLevel <= 8 && detailLevel >= 0)
        {
            if (Vector3.Distance(position.normalized * Planet.size, Planet.player.position) <= Planet.detailLevelDistances[detailLevel])
            {
                // Assign the chunks children (grandchildren not included). 
                // Position is calculated on a cube and based on the fact that each child has 1/2 the radius of the parent
                // Detail level is increased by 1. This doesn't change anything itself, but rather symbolizes that something HAS been changed (the detail).
                children = new Chunk[4];
                children[0] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[1] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[2] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[3] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);

                // Create grandchildren
                foreach (Chunk child in children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }

    // Returns the latest chunk in every branch, aka the ones to be rendered
    public Chunk[] GetVisibleChildren()
    {
        List<Chunk> toBeRendered = new List<Chunk>();

        if (children.Length > 0)
        {
            foreach (Chunk child in children)
            {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        } else
        {
            toBeRendered.Add(this);
        }

        return toBeRendered.ToArray();
    }

    // Most of this code comes from Sebatian Lague
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset)
    {
        int resolution = 8; // The resolution of the chunk. Can be changed
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                /* Same code as Sebastian Lague, with the difference being that
                1: The origin is the position variable rather than the middle of the terrain face
                2: The offset is scaled using the radius variable */
                Vector3 pointOnUnitCube = position + ((percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB) * radius;


                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * Planet.size; // Inflate the cube by projected the vertices onto a sphere with the size of Planet.size
                vertices[i] = pointOnUnitSphere;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i + triangleOffset;
                    triangles[triIndex + 1] = i + resolution + 1 + triangleOffset;
                    triangles[triIndex + 2] = i + resolution + triangleOffset;

                    triangles[triIndex + 3] = i + triangleOffset;
                    triangles[triIndex + 4] = i + 1 + triangleOffset;
                    triangles[triIndex + 5] = i + resolution + 1 + triangleOffset;

                    triIndex += 6;
                }
            }
        }

        return (vertices, triangles);
    }
}