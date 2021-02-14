using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerrainFace
{
    public volatile Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;
    public Chunk parentChunk;
    public Planet planetScript;
    public List<Chunk> visibleChildren = new List<Chunk>();

    // These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> borderVertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<int> triangles = new List<int>();
    public List<int> borderTriangles = new List<int>();
    public List<Color> colors = new List<Color>();
    public Dictionary<int, bool> edgefanIndex = new Dictionary<int, bool>();
    public Texture2D texture;
    public int textureWidth = 1080;
    public int textureBorder = 5;

    // Constructor
    public TerrainFace(Mesh mesh, Vector3 localUp, float radius, Planet planetScript)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.radius = radius;
        this.planetScript = planetScript;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisA.RoundToWorldAxis();
        axisB = Vector3.Cross(localUp, axisA);
        axisB.RoundToWorldAxis();
    }

    // Update the quadtree
    public void GenerateTree(bool multithread = false)
    {
        // Resets the mesh
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        uvs.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();
        edgefanIndex.Clear();
        colors.Clear();

        if(parentChunk == null) {
            parentChunk = new Chunk(1, this, (DVector3) localUp.normalized * planetScript.Size, radius, 0, (DVector3) localUp, (DVector3) axisA, (DVector3) axisB, 0);
            parentChunk.GenerateChildren();
        } else {
            parentChunk.UpdateChunk();
        }

        // Get chunk mesh data
        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        foreach (Chunk child in visibleChildren)
        {
            Vector3[] newVertices;
            int[] newTriangles;
            int[] newBorderTriangles;
            Vector3[] newBorderVertices;
            Vector3[] newNormals;
            Vector2[] newUVs;
            Color[] newColors;
            child.GetNeighbourLOD();

            if (child.vertices == null)
            {
                child.CalculateMeshProperties(triangleOffset, borderTriangleOffset, 
                out newVertices, out newTriangles, out newBorderTriangles, out newBorderVertices, out newNormals, out newUVs, out newColors);
            }
            else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[child.neighbours.AsBinarySequence(4)])
            {
                child.CalculateMeshProperties(triangleOffset, borderTriangleOffset, 
                out newVertices, out newTriangles, out newBorderTriangles, out newBorderVertices, out newNormals, out newUVs, out newColors);
            }
            else
            {
                newVertices = child.vertices;
                newTriangles = child.GetTrianglesWithOffset(triangleOffset);
                newBorderTriangles = child.GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset);
                newBorderVertices = child.borderVertices;
                newNormals = child.normals;
                newUVs = child.uvs;
                newColors = child.colors;
            }

            vertices.AddRange(newVertices);
            triangles.AddRange(newTriangles);
            borderTriangles.AddRange(newBorderTriangles);
            borderVertices.AddRange(newBorderVertices);
            normals.AddRange(newNormals);
            uvs.AddRange(newUVs);
            colors.AddRange(newColors);

            // Increase offset to accurately point to the next slot in the lists
            triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
            borderTriangleOffset += newBorderVertices.Length;
        }

        if(multithread) {
            lock(planetScript._asyncLock) {
                planetScript.ActionQueue.Add(() => {
                    UpdateMesh(mesh, vertices.ToArray(), triangles.ToArray(), normals.ToArray(), uvs.ToArray(), colors.ToArray());
                });
            }
        } else {
            lock(planetScript._asyncLock) {
                UpdateMesh(mesh, vertices.ToArray(), triangles.ToArray(), normals.ToArray(), uvs.ToArray(), colors.ToArray());
            }
        }
    }

    void UpdateMesh(Mesh mesh, Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uvs, Color[] colors) {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateBounds();
    }

    public Color[] GenerateTextureData(int width, int border)
    {
        int textureWidthWithBorder = width + border * 2;
        Color[] pix = new Color[textureWidthWithBorder * textureWidthWithBorder];
        
        // Create a buffer of a couple pixel(s) at the edges of the texture
        for (int y = 0; y < textureWidthWithBorder; y++)
        {
            for (int x = 0; x < textureWidthWithBorder; x++)
            {
                float pointX = Mathf.Clamp((x - textureWidthWithBorder * 0.5f) / (width * 0.5f), -1, 1);
                float pointY = Mathf.Clamp((y - textureWidthWithBorder * 0.5f) / (width * 0.5f), -1, 1);
                float valuex = Mathf.Abs(x - textureWidthWithBorder * 0.5f) - width * 0.5f;
                float valuey = Mathf.Abs(y - textureWidthWithBorder * 0.5f) - width * 0.5f;
                float value = Mathf.Max(Mathf.Max(valuex, valuey), 0);
                float pointZ = value / (width * 0.5f);
                Vector3 point = axisB * pointX + axisA * pointY - localUp * pointZ;
                Vector3 pointOnUnitSphere = (localUp + point).normalized;
                float elevation = GetElevation(planetScript.LowDefElevationConfig, pointOnUnitSphere);
                float yRatio = (pointOnUnitSphere.y + 1) * 0.5f;
                Color clr = GetBiomeColor(planetScript.BiomeConfig, elevation, GetMaxHeight(planetScript.LowDefElevationConfig), pointOnUnitSphere, yRatio);
                pix[y * textureWidthWithBorder + x] = clr;
            }
        }

        return pix;
    }

    public void SetTexture(int width, int border, Color[] data) {
        int textureWidthWithBorder = width + border * 2;
        texture = new Texture2D(textureWidthWithBorder, textureWidthWithBorder, TextureFormat.RGB24, false, false);
        texture.SetPixels(data, 0);
        texture.Apply();
    }

    public static float GetElevation(TerrainConfig config, Vector3 pointOnUnitSphere) {
        float elevation = 0;

        for(int i = 0; i < config.NoiseFilters.Length; i++) {
            elevation += config.NoiseFilters[i].Evaluate(pointOnUnitSphere);
        }

        return elevation;
    }

    public static double GetElevationD(TerrainConfig config, Vector3 pointOnUnitSphere) {
        double elevation = 0;

        for(int i = 0; i < config.NoiseFilters.Length; i++) {
            elevation += config.NoiseFilters[i].EvaluateD(pointOnUnitSphere);
        }

        return elevation;
    }

    /// <summary>
    /// Get the maximum elevation value.
    /// </summary>
    public static float GetMaxHeight(TerrainConfig config) {
        float maxHeight = 0;

        for(int i = 0; i < config.NoiseFilters.Length; i++) {
            NoiseFilter noiseFilter = config.NoiseFilters[i];
            if(noiseFilter.clampingEnabled) {
                maxHeight += noiseFilter.strength * Mathf.Clamp(noiseFilter.multiplier, noiseFilter.minValue, noiseFilter.maxValue);
            } else {
                maxHeight += noiseFilter.strength;
            }
        }

        return maxHeight;
    }

    /// <summary>
    /// Get the surface color considering biome and height.
    /// </summary>
    public Color GetBiomeColor(BiomeConfig biomeConfig, float elevation, float maxHeight, Vector3 pointOnUnitSphere, float ratio) {
        Noise noise = new Noise();
        float biomeNoise = (noise.Evaluate(pointOnUnitSphere * biomeConfig.noiseScale) - biomeConfig.offset) * biomeConfig.distortion;
        float sampleTime = Mathf.Clamp(ratio + biomeNoise, 0.01f, 9.99f);
        float biomeValue = biomeConfig.gradient.Evaluate(sampleTime).r;

        #region explanation
        // STEP 0: Get theg gradients (Doesn't have to be 3)
        // ----------, ----------, ----------

        // STEP 1: Stack the gradients ontop of eachother
        // ---------- (0)
        // ---------- (1)
        // ---------- (2)

        // STEP 2: Create a linear transition between, i.e smudge the borders
        // ~~~~~~~~~~ (0)
        // ~~~~~~~~~~ (1)
        // ~~~~~~~~~~ (2)

        // STEP 3: Select a horizontal slice of the stack at a specified depth. This is the final gradient.
        // ~~~~~~~~~~ (0)
        // ~~~~~~~~~~ (1)
        // ~~~~~~~~~~ (2)
        // SELECTED GRADIENT (----------) AT DEPTH 1.35 
        #endregion
        
        Gradient gradient = new Gradient();
        Biome[] biomes = biomeConfig.biomes;
        GradientColorKey[] colorKeys = new GradientColorKey[biomes.Length];
        for(int j = 0; j < colorKeys.Length; j++) {
            colorKeys[j].time = biomes[j].order;
            colorKeys[j].color = biomes[j].gradient.Evaluate(elevation / maxHeight);
        }
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[1].alpha = 1.0f;
        gradient.SetKeys(colorKeys, alphaKeys);

        return gradient.Evaluate(biomeValue);
    }
}