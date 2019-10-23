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
    Chunk parentChunk;
    public Planet planetScript;

    // These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    // Constructor
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, float radius, Planet planetScript)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.radius = radius;
        this.planetScript = planetScript;

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
        parentChunk = new Chunk(planetScript, null, null, localUp.normalized * planetScript.size, radius, 0, localUp, axisA, axisB);
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

    // Update the quadtree
    public void UpdateTree()
    {
        // Resets the mesh
        vertices.Clear();
        triangles.Clear();

        parentChunk.UpdateChunk();

        // Get chunk mesh data
        int triangleOffset = 0;
        foreach (Chunk child in parentChunk.GetVisibleChildren())
        {
            (Vector3[], int[]) verticesAndTriangles = (new Vector3[0], new int[0]);
            if (child.vertices == null)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else if(child.vertices.Length == 0)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else
            {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset));
            }

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
    public Planet planetScript;

    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;

    public Vector3[] vertices;
    public int[] triangles;

    // Constructor
    public Chunk(Planet planetScript, Chunk[] children, Chunk parent, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB)
    {
        this.planetScript = planetScript;
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
        int maxDetail = 8;

        // If the detail level is under max level and above 0. Max level depends on how many detail levels are defined in planets and needs to be changed manually.
        if (detailLevel <= maxDetail && detailLevel >= 0)
        {
            if (Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size), planetScript.player.position) <= planetScript.detailLevelDistances[detailLevel])
            {
                // Assign the chunks children (grandchildren not included). 
                // Position is calculated on a cube and based on the fact that each child has 1/2 the radius of the parent
                // Detail level is increased by 1. This doesn't change anything itself, but rather symbolizes that something HAS been changed (the detail).
                children = new Chunk[4];
                children[0] = new Chunk(planetScript, new Chunk[0], this, position + axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[1] = new Chunk(planetScript, new Chunk[0], this, position + axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[2] = new Chunk(planetScript, new Chunk[0], this, position - axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[3] = new Chunk(planetScript, new Chunk[0], this, position - axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);

                // Create grandchildren
                foreach (Chunk child in children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }

    // Update the chunk (and maybe its childrent too)
    public void UpdateChunk()
    {
        float distanceToPlayer = Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size), planetScript.player.position);
        if (detailLevel <= 8) {
            if (distanceToPlayer > planetScript.detailLevelDistances[detailLevel])
            {
                children = new Chunk[0];
            }
            else
            {
                if (children.Length > 0)
                {
                    foreach (Chunk child in children)
                    {
                        child.UpdateChunk();
                    }
                }
                else
                {
                    GenerateChildren();
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
            if (Mathf.Acos((Mathf.Pow(planetScript.size, 2) + Mathf.Pow(planetScript.distanceToPlayer, 2) - 
               Mathf.Pow(Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size), planetScript.player.position), 2)) / 
               (2 * planetScript.size * planetScript.distanceToPlayer)) < planetScript.cullingMinAngle)
            {
                toBeRendered.Add(this);
            }
        }

        return toBeRendered.ToArray();
    }

    // Return triangles including offset
    public int[] GetTrianglesWithOffset(int triangleOffset)
    {
        int[] triangles = new int[this.triangles.Length];

        for(int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = this.triangles[i] + triangleOffset;
        }

        return triangles;
    }

    // A lot of this code comes from Sebastian Lague
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset)
    {
        int resolution = 9; // The resolution of the chunk. Can be changed but must be odd
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
                Vector3 pointOnUnitCube = position + ((percent.x - .5f) * axisA + (percent.y - .5f) * axisB) * 2 * radius;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * planetScript.size; // Inflate the cube by projecting the vertices onto a sphere with the size of Planet.size
                vertices[i] = pointOnUnitSphere;

                if (x < resolution - 1 && y < resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;

                    triangles[triIndex + 5] = i + resolution + 1;

                    triIndex += 6;
                }
            }
        }

        // Store the vertices and triangles
        this.vertices = vertices;
        this.triangles = triangles;

        return (vertices, GetTrianglesWithOffset(triangleOffset));
    }
}