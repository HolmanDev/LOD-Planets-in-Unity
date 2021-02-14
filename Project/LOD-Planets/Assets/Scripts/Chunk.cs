using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public uint hashvalue; // First bit is not used for anything but preserving zeros in the beginning
    public Planet planetScript;
    public TerrainFace terrainFace;

    public Chunk[] children;
    public DVector3 position; // Origin is the planet, direction is world
    public double radius;
    public int detailLevel;
    public DVector3 localUp;
    public DVector3 axisA; // y
    public DVector3 axisB; // z
    public byte corner;

    public Vector3 normalizedPos;

    public Vector3[] vertices;
    public Vector3[] borderVertices;
    public int[] triangles;
    public int[] borderTriangles;
    public Vector3[] normals;
    public Vector2[] uvs;
    public Color[] colors;

    public bool[] neighbours = new bool[4]; //East, west, north, south. True if less detailed (Lower LOD).

    /// <summary> 
    /// Neighbour chunk indexer. 
    /// </summary>
    public static class Direction {
        public const int East = 0, West = 1, North = 2, South = 3;
        /// <summary> East = 0, West = 1, North = 2, South = 3 </summary>
        public const int E = 0, W = 1, N = 2, S = 3;
    }

    /// <summary> 
    /// Child chunk indexer. 
    /// </summary>
    public static class Quadrant {
        public const int NorthWest = 0, NorthEast = 1, SouthEast = 2, SouthWest = 3;
        /// <summary> North West = 0, North East = 1, South East = 2, South West = 3 </summary>
        public const int NW = 0, NE = 1, SE = 2, SW = 3;
    }

    public Chunk(uint hashvalue, TerrainFace terrainFace, DVector3 position, double radius, int detailLevel, DVector3 localUp, DVector3 axisA, DVector3 axisB, byte corner)
    {
        this.hashvalue = hashvalue;
        this.terrainFace = terrainFace;
        this.planetScript = terrainFace.planetScript;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.corner = corner;
        this.normalizedPos = (Vector3) position.normalized; // ?
        this.children = new Chunk[0]; // remove from constructor
        this.neighbours = new bool[4]; // remove from constructor
    }

    public void GenerateChildren()
    {
        // If the detail level is under max level and above 0. Max level depends on how many detail levels are defined in planets and needs to be changed manually.
        if (detailLevel <= planetScript.detailLevelDistances.Length - 1 && detailLevel >= 0)
        {
            float elevation = TerrainFace.GetElevation(planetScript.LowDefElevationConfig, normalizedPos);

            Vector3 worldPos = normalizedPos * planetScript.Size * (1 + elevation) + planetScript.position;
            Vector3 boundsSize = new Vector3((float) radius * 2, (float) radius * 2, 1);
            Bounds bounds = new Bounds(Vector3.zero, boundsSize);

            //Convert the players position so that it is relative to the quad, both position and rotation wise
            Vector3 worldDirection = normalizedPos;
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.FromToRotation(worldDirection, Vector3.forward));
            Vector3 playerPos = rotationMatrix.MultiplyPoint(planetScript.PlayerPos - worldPos);

            if (PlayerWithinRange(planetScript.detailLevelDistances[detailLevel] * planetScript.detailLevelDistances[detailLevel], playerPos, bounds))
            {
                // Assign the children of the quad (grandchildren not included). 
                // Position is calculated on a cube and based on the fact that each child has 1/2 the radius of its parent
                // Detail level is increased by 1. This doesn't change anything itself, but rather symbolizes that something HAS been changed (the detail).
                double halfRadius = radius * 0.5;

                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4,     terrainFace, position + (axisA - axisB) * halfRadius, halfRadius, detailLevel + 1, localUp, axisA, axisB, Quadrant.NW); // TOP LEFT
                children[1] = new Chunk(hashvalue * 4 + 1, terrainFace, position + (axisA + axisB) * halfRadius, halfRadius, detailLevel + 1, localUp, axisA, axisB, Quadrant.NE); // TOP RIGHT
                children[2] = new Chunk(hashvalue * 4 + 2, terrainFace, position + (axisB - axisA) * halfRadius, halfRadius, detailLevel + 1, localUp, axisA, axisB, Quadrant.SE); // BOTTOM RIGHT
                children[3] = new Chunk(hashvalue * 4 + 3, terrainFace, position - (axisB + axisA) * halfRadius, halfRadius, detailLevel + 1, localUp, axisA, axisB, Quadrant.SW); // BOTTOM LEFT

                // Create grandchildren
                foreach (Chunk child in children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }

    // Update the chunk (and maybe its children too)
    public void UpdateChunk()
    {
        float elevation = TerrainFace.GetElevation(planetScript.LowDefElevationConfig, normalizedPos);
        
        Vector3 worldPos = normalizedPos * planetScript.Size * (1 + elevation) + planetScript.position;
        Vector3 boundsSize = new Vector3((float) radius * 2, (float) radius * 2, 1);
        Bounds bounds = new Bounds(Vector3.zero, boundsSize);

        //Convert the players position so that it is relative to the quad, both position and rotation wise
        Vector3 worldDirection = normalizedPos;
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.FromToRotation(worldDirection, Vector3.forward));
        Vector3 playerPos = rotationMatrix.MultiplyPoint(planetScript.PlayerPos - worldPos);

        if (detailLevel <= planetScript.detailLevelDistances.Length - 1)
        {
            if (!PlayerWithinRange(planetScript.detailLevelDistances[detailLevel] * planetScript.detailLevelDistances[detailLevel], playerPos, bounds))
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

    private bool PlayerWithinRange(float sqrDistance, Vector3 playerPos, Bounds bounds) {
        return bounds.SqrDistance(playerPos) < sqrDistance;
    }

    // Returns the latest chunk in every branch, i.e the ones to be rendered
    public void GetVisibleChildren()
    {
        if (children.Length > 0)
        {
            foreach (Chunk child in children)
            {
                child.GetVisibleChildren();
            }
        }
        else
        {

            float b = Vector3.Distance(normalizedPos * planetScript.Size +
                planetScript.position, planetScript.PlayerPos);

            if ((Mathf.Acos(((planetScript.Size * planetScript.Size) + (b * b) -
                planetScript.distanceToPlayerPow2) / (2 * planetScript.Size * b)) > planetScript.CullingMinAngle) || !planetScript.CullingEnabled)
            {
                terrainFace.visibleChildren.Add(this);
            }
        }
    }

    public void GetNeighbourLOD()
    {
        bool[] newNeighbours = new bool[4];

        if (corner == 0) // Top left
        {
            newNeighbours[Direction.West] = CheckNeighbourLOD(Direction.West, hashvalue); // West
            newNeighbours[Direction.North] = CheckNeighbourLOD(Direction.North, hashvalue); // North
        }
        else if (corner == 1) // Top right
        {
            newNeighbours[Direction.East] = CheckNeighbourLOD(Direction.East, hashvalue); // East
            newNeighbours[Direction.North] = CheckNeighbourLOD(Direction.North, hashvalue); // North
        }
        else if (corner == 2) // Bottom right
        {
            newNeighbours[Direction.East] = CheckNeighbourLOD(Direction.East, hashvalue); // East
            newNeighbours[Direction.South] = CheckNeighbourLOD(Direction.South, hashvalue); // South
        }
        else if (corner == 3) // Bottom left
        {
            newNeighbours[Direction.West] = CheckNeighbourLOD(Direction.West, hashvalue); // West
            newNeighbours[Direction.South] = CheckNeighbourLOD(Direction.South, hashvalue); // South
        }

        neighbours = newNeighbours;
    }

    // Find neighbouring chunks LOD at slot by applying a partial inverse bitmask to the hash.
    private bool CheckNeighbourLOD(int direction, uint hash)
    {
        uint bitmask = 0;
        byte count = 0;
        uint localChunkQuadrant;

        // WILL A FOR LOOP RUN FASTER?
        while (count < detailLevel * 2) // 0 through 3 can be represented as a two bit number
        {
            count += 2;
            localChunkQuadrant = (hash & 3); // Get the two last bits of the hash. 0b_10011 --> 0b_11

            bitmask = bitmask * 4; // Add zeroes to the end of the bitmask. 0b_10011 --> 0b_1001100

            // Create mask to get the quad on the opposite side. 2 = 0b_10 and generates the mask 0b_11 which flips it to 1 = 0b_01
            if (direction == 2 || direction == 3)
            {
                bitmask += 3; // Add 0b_11 to the bitmask
            }
            else
            {
                bitmask += 1; // Add 0b_01 to the bitmask
            }

            // Break if the hash goes in the opposite direction
            if ((direction == Direction.E && (localChunkQuadrant == Quadrant.NW || localChunkQuadrant == Quadrant.SW)) ||
                (direction == Direction.W && (localChunkQuadrant == Quadrant.NE || localChunkQuadrant == Quadrant.SE)) ||
                (direction == Direction.N && (localChunkQuadrant == Quadrant.SW || localChunkQuadrant == Quadrant.SE)) ||
                (direction == Direction.S && (localChunkQuadrant == Quadrant.NW || localChunkQuadrant == Quadrant.NE)))
            {
                break;
            }

            // Remove already processed bits. 0b_1001100 --> 0b_10011
            hash = hash >> 2;
        }

        // Return true if the quad in quadstorage is less detailed. REACH BEYOND THIS FACE IF THE CHUNK IS ON THE FACE'S BORDER.
        return terrainFace.parentChunk.GetNeighbourDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel;
    }

    // Find the detail level of the neighbouring quad using the querryHash as a map
    public int GetNeighbourDetailLevel(uint querryHash, int dl)
    {
        int dlResult = 0; // dl = detail level

        if (hashvalue == querryHash)
        {
            dlResult = detailLevel;
        }
        else
        {
            if (children.Length > 0)
            {
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDetailLevel(querryHash, dl - 1);
            }
        }

        return dlResult; // Returns 0 if no quad with the given hash is found
    }

    // Return triangles including offset
    public int[] GetTrianglesWithOffset(int triangleOffset)
    {
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            newTriangles[i] = triangles[i] + triangleOffset;
        }

        return newTriangles;
    }

    // Return border triangles including offset
    public int[] GetBorderTrianglesWithOffset(int borderTriangleOffset, int triangleOffset)
    {
        int[] newBorderTriangles = new int[borderTriangles.Length];

        for (int i = 0; i < borderTriangles.Length; i++)
        {
            newBorderTriangles[i] = (borderTriangles[i] < 0) ? borderTriangles[i] - borderTriangleOffset : borderTriangles[i] + triangleOffset;
        }

        return newBorderTriangles;
    }

    /// <summary>
    /// Calculate vertices, triangles, normals and other mesh properties.
    /// </summary>
    public void CalculateMeshProperties(int triangleOffset, int borderTriangleOffset, 
    out Vector3[] newVertices, out int[] newTriangles, out int[] newBorderTriangles, out Vector3[] newBorderVertices, out Vector3[] newNormals, out Vector2[] newUVs, out Color[] newColors)
    {
        DVector3 scale = new DVector3(radius, 1, radius);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(Vector3.zero));
        // Adjust rotation according to the side of the planet
        if (terrainFace.localUp == Vector3.forward)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, 180)));
        }
        else if (terrainFace.localUp == Vector3.back)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 180, 0)));
        }
        else if (terrainFace.localUp == Vector3.right)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 90, 270)));
        }
        else if (terrainFace.localUp == Vector3.left)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 270, 270)));
        }
        else if (terrainFace.localUp == Vector3.up)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(270, 0, 90)));
        }
        else if (terrainFace.localUp == Vector3.down)
        {
            rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(90, 0, 270)));
        }

        // Index of quad template
        int quadIndex = neighbours.AsBinarySequence(4);

        // Choose a quad from the templates, then move it using the transform matrix, normalize its vertices, scale it and store it
        vertices = new Vector3[(Presets.quadRes + 1) * (Presets.quadRes + 1)];
        colors = new Color[(Presets.quadRes + 1) * (Presets.quadRes + 1)];

        Vector2 uv = new Vector2(detailLevel / 16f, 0);
        uvs = (new Vector2[vertices.Length]).Populate(uv);

        int[] edgefansIndices = Presets.quadTemplateEdgeIndices[quadIndex];

        for (int i = 0; i < vertices.Length; i++)
        {
            DVector3 pointOnCube = GetPointOnCube((DVector3) Presets.quadTemplateVertices[quadIndex][i], scale, position, rotationMatrix);
            Vector3 pointOnUnitSphere = (Vector3) pointOnCube.normalized;
            float elevation = TerrainFace.GetElevation(planetScript.HighDefElevationConfig, pointOnUnitSphere) + 
                TerrainFace.GetElevation(planetScript.LowDefElevationConfig, pointOnUnitSphere);
            vertices[i] = pointOnUnitSphere * ((1f + elevation) * planetScript.Size);

            #region TIP: How to calculate point on unit cube using point on unit sphere
            // P = point on unit cube
            // v̂ = chunk forward unit vector
            // û = vertex vector on unit sphere (or unit cube, doesn't matter)
            // θ = angle between û and v̂
            // P = û * r / cos(θ)
            // r = 1, so the previous line can be simplified
            // P = û / cos(θ)
            // Here it is verbose...
            // Point on unit cube = point on unit sphere / cos(angle between chunk forward unit vector and point on unit sphere)
            // ...and in code...
            // Vector3 P = pointOnUnitSphere / Mathf.Cos(Vector3.Angle(pointOnUnitSphere, localUp) * Mathf.Deg2Rad);
            #endregion
            // Vertex UV
            Vector3 pointOnUnitCube = (Vector3) pointOnCube / planetScript.Size;
            float x = Vector3.Dot(pointOnUnitCube, (Vector3) axisB);
            float y = Vector3.Dot(pointOnUnitCube, (Vector3) axisA);
            float scaleFactor = terrainFace.textureWidth / (terrainFace.textureWidth + terrainFace.textureBorder * 2f); // Fixes some of the weirdness at the borders. This is a sub-optimal "fix".
            uvs[i] = new Vector2((x * scaleFactor + 1) * 0.5f, (y * scaleFactor + 1) * 0.5f);

            // Don't bother with vertex colors if they are covered by texture colors.
            if(detailLevel >= planetScript.vertexColorMinLOD) {
                float yRatio = (pointOnUnitSphere.y + 1) * 0.5f;
                float maxHeight = TerrainFace.GetMaxHeight(planetScript.HighDefElevationConfig) + TerrainFace.GetMaxHeight(planetScript.LowDefElevationConfig);
                colors[i] = terrainFace.GetBiomeColor(planetScript.BiomeConfig, elevation, maxHeight, pointOnUnitSphere, yRatio);
            } else {
                colors[i] = new Color(1,0,1,1); // Vertex color is pink by default
            }
        }

        // Do the same for the border vertices
        borderVertices = new Vector3[Presets.quadTemplateBorderVertices[quadIndex].Length];

        for (int i = 0; i < borderVertices.Length; i++)
        {
            DVector3 pointOnCube = GetPointOnCube((DVector3) Presets.quadTemplateBorderVertices[quadIndex][i], scale, position, rotationMatrix);
            Vector3 pointOnUnitSphere = (Vector3) pointOnCube.normalized;
            float elevation = TerrainFace.GetElevation(planetScript.HighDefElevationConfig, pointOnUnitSphere) + 
                TerrainFace.GetElevation(planetScript.LowDefElevationConfig, pointOnUnitSphere);
            borderVertices[i] = pointOnUnitSphere * ((1f + elevation) * planetScript.Size);
        }

        // Store the triangles
        triangles = Presets.quadTemplateTriangles[quadIndex];
        borderTriangles = Presets.quadTemplateBorderTriangles[quadIndex];

        // MASSIVE CREDIT TO SEBASTIAN LAGUE FOR PROVIDING THE FOUNDATION FOR THE FOLLOWING CODE
        // Calculate the normals
        normals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;

        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;

        Vector3 triangleNormal;

        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Don't calculate the normals on the edge edgefans here. They are only calculated using the border vertices.
            if (edgefansIndices[vertexIndexA] == 0)
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexB] == 0)
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexC] == 0)
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            vertexIndexA = borderTriangles[normalTriangleIndex];
            vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Apply the normal if the vertex is on the visible edge of the quad
            if (vertexIndexA >= 0 && (vertexIndexA % (Presets.quadRes + 1) == 0 ||
                vertexIndexA % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexA >= 0 && vertexIndexA <= Presets.quadRes) ||
                (vertexIndexA >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexA < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0 && (vertexIndexB % (Presets.quadRes + 1) == 0 ||
                vertexIndexB % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexB >= 0 && vertexIndexB <= Presets.quadRes) ||
                (vertexIndexB >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexB < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0 && (vertexIndexC % (Presets.quadRes + 1) == 0 ||
                vertexIndexC % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexC >= 0 && vertexIndexC <= Presets.quadRes) ||
                (vertexIndexC >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexC < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        // Normalize the result to combine the approximations into one
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        newVertices = vertices;
        newTriangles = GetTrianglesWithOffset(triangleOffset);
        newBorderTriangles = GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset);
        newBorderVertices = borderVertices;
        newNormals = normals;
        newUVs = uvs;
        newColors = colors;
    }

    private DVector3 GetPointOnCube(DVector3 vector, DVector3 scale, DVector3 translation, Matrix4x4 rotation) {
        vector.Scale(scale);
        vector = (DVector3) rotation.MultiplyPoint((Vector3) vector);
        vector.Translate(position);
        return vector;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        // Get an aproximation of the vertex normal using two other vertices that share the same triangle
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}
