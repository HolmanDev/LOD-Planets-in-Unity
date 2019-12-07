using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;
    public Chunk parentChunk;
    public Planet planetScript;

    // These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    // Constructor
    public TerrainFace(Mesh mesh, Vector3 localUp, float radius, Planet planetScript)
    {
        this.mesh = mesh;
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
        parentChunk = new Chunk(1, planetScript, this, null, localUp.normalized * planetScript.size, radius, 0, localUp, axisA, axisB, new byte[4], 0);
        parentChunk.GenerateChildren();

        // Get chunk mesh data
        int triangleOffset = 0;
        foreach(Chunk child in parentChunk.GetVisibleChildren())
        {
            child.GetNeighbourLOD();
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
            child.GetNeighbourLOD();
            (Vector3[], int[]) verticesAndTriangles = (new Vector3[0], new int[0]);
            if (child.vertices == null)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)])
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else//Check if neighbour LODS are the same or not
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
    public uint hashvalue; // First bit is not used for anything but preserving zeros in the beginning
    public Planet planetScript;
    public TerrainFace terrainFace;

    public Chunk[] children;
    public Vector3 position;
    public float radius;
    public byte detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public byte corner;

    public Vector3[] vertices;
    public int[] triangles;

    public byte[] neighbours = new byte[4]; //East, west, north, south. True if less detailed (Lower LOD)
    // Constructor
    public Chunk(uint hashvalue, Planet planetScript, TerrainFace terrainFace, Chunk[] children, Vector3 position, float radius, byte detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner)
    {
        this.hashvalue = hashvalue;
        this.planetScript = planetScript;
        this.terrainFace = terrainFace;
        this.children = children;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.neighbours = neighbours;
        this.corner = corner;
    }

    public void GenerateChildren()
    {
        // If the detail level is under max level and above 0. Max level depends on how many detail levels are defined in planets and needs to be changed manually.
        if (detailLevel <= planetScript.detailLevelDistances.Length - 1 && detailLevel >= 0)
        {
            if (Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size) + planetScript.transform.position, planetScript.player.position) <= planetScript.detailLevelDistances[detailLevel])
            {
                // Assign the chunks children (grandchildren not included). 
                // Position is calculated on a cube and based on the fact that each child has 1/2 the radius of the parent
                // Detail level is increased by 1. This doesn't change anything itself, but rather symbolizes that something HAS been changed (the detail).
                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4, planetScript, terrainFace, new Chunk[0], position + axisA * radius / 2 - axisB * radius / 2, radius / 2, (byte) (detailLevel + 1), localUp, axisA, axisB, new byte[4], 0); // TOP LEFT
                children[1] = new Chunk(hashvalue * 4 + 1, planetScript, terrainFace, new Chunk[0], position + axisA * radius / 2 + axisB * radius / 2, radius / 2, (byte)(detailLevel + 1), localUp, axisA, axisB, new byte[4], 1); // TOP RIGHT
                children[2] = new Chunk(hashvalue * 4 + 2, planetScript, terrainFace, new Chunk[0], position - axisA * radius / 2 + axisB * radius / 2, radius / 2, (byte)(detailLevel + 1), localUp, axisA, axisB, new byte[4], 2); // BOTTOM RIGHT
                children[3] = new Chunk(hashvalue * 4 + 3, planetScript, terrainFace, new Chunk[0], position - axisA * radius / 2 - axisB * radius / 2, radius / 2, (byte)(detailLevel + 1), localUp, axisA, axisB, new byte[4], 3); // BOTTOM LEFT

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
        float distanceToPlayer = Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size) + planetScript.transform.position, planetScript.player.position);
        if (detailLevel <= planetScript.detailLevelDistances.Length - 1) {
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
               Mathf.Pow(Vector3.Distance(planetScript.transform.TransformDirection(position.normalized * planetScript.size) + planetScript.transform.position, planetScript.player.position), 2)) / 
               (2 * planetScript.size * planetScript.distanceToPlayer)) < planetScript.cullingMinAngle)
            {
                toBeRendered.Add(this);
            }
        }

        return toBeRendered.ToArray();
    }

    public void GetNeighbourLOD()
    {
        neighbours = new byte[4];

        if(corner == 0) // Top left
        {
            neighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            neighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 1) // Top right
        {
            neighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            neighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 2) // Bottom right
        {
            neighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            neighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        } else if(corner == 3) // Bottom left
        {
            neighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            neighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        }
    }

    // Find neighbouring chunks by applying a partial inverse bitmask to the hash
    private byte CheckNeighbourLOD(byte side, uint hash)
    {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < detailLevel * 2) // 0 through 3 can be represented as a two bit number
        {
            count+=2;
            twoLast = (hash & 3); // Get the two last bits of the hash. 0b_10011 --> 0b_11

            bitmask = bitmask * 4; // Add zeroes to the end of the bitmask. 0b_10011 --> 0b_1001100

            // Create mask to get the quad on the opposite side. 2 = 0b_10 and generates the mask 0b_11 which flips it to 1 = 0b_01
            if (side == 2 || side == 3)
            {
                bitmask += 3; // Add 0b_11 to the bitmask
            }
            else
            {
                bitmask += 1; // Add 0b_01 to the bitmask
            }

            // Break if the hash goes in the opposite direction
            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1)))
            {
                break;
            }

            // Remove already processed bits. 0b_1001100 --> 0b_10011
            hash = hash >> 2;
        }

        // Return 1 (true) if the quad in quadstorage is less detailed
        if (terrainFace.parentChunk.GetQuadDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    // Find the detail level of the neighbouring quad using the querryHash as a map
    public byte GetQuadDetailLevel(uint querryHash, byte dl)
    {
        byte dlResult = 0; // dl = detail level

        if (hashvalue == querryHash)
        {
            dlResult = detailLevel;
        } else
        {
            if (children.Length > 0)
            {
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetQuadDetailLevel(querryHash, (byte)(dl - 1));
            }
        }

        return dlResult; // Returns 0 if no quad with the given hash is found
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

    // A lot of this code comes from Sebastian Lague. NOTE!!! IT IS CURRENTLY FLIPPED ON EVERYSIDE BUT THE FRONT
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset)
    {
        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0,0,0);
        Vector3 flipMatrixAttrib = new Vector3(1,1,1);
        Vector3 scaleMatrixAttrib = new Vector3(radius, radius, 1);

        // Adjust rotation according to the side of the planet
        if(terrainFace.localUp == Vector3.forward)
        {
            rotationMatrixAttrib = new Vector3(0, 0, 180);
        } else if (terrainFace.localUp == Vector3.back)
        {
            rotationMatrixAttrib = new Vector3(0, 180, 0);
        } else if (terrainFace.localUp == Vector3.right)
        {
            rotationMatrixAttrib = new Vector3(0, 90, 270);
        }
        else if(terrainFace.localUp == Vector3.left)
        {
            rotationMatrixAttrib = new Vector3(0, 270, 270);
        } else if (terrainFace.localUp == Vector3.up)
        {
            rotationMatrixAttrib = new Vector3(270, 0, 90);
        }
        else if(terrainFace.localUp == Vector3.down)
        {
            rotationMatrixAttrib = new Vector3(90, 0, 270);
        }

        // Create transform matrix
        transformMatrix = Matrix4x4.TRS(position, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);

        // Index of quad template
        int quadIndex = (neighbours[0] | neighbours[1] * 2 | neighbours[2] * 4 | neighbours[3] * 8);

        // Choose a quad from the templates, then move it using the transform matrix, normalize its vertices, scale it and store it
        vertices = new Vector3[(Presets.quadRes + 1) * (Presets.quadRes + 1)];

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transformMatrix.MultiplyPoint(Presets.quadTemplateVertices[quadIndex][i]).normalized * planetScript.size;
        }

        // Store the triangles
        triangles = Presets.quadTemplateTriangles[quadIndex];

        return (vertices, GetTrianglesWithOffset(triangleOffset));
    }
}