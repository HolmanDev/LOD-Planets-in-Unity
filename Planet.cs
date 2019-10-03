using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Most of the code in this script is made by Sebastian Lague, except for the LOD stuff
public class Planet : MonoBehaviour
{
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public static float size = 10; // Must be set to the size of the planet defined in the inspector

    public static Transform player;

    // Hardcoded detail levels. First value is level, second is distance from player. Finding the right values can be a little tricky
    public static Dictionary<int, float> detailLevelDistances = new Dictionary<int, float>() {
        {0, Mathf.Infinity },
        {1, 60f},
        {2, 25f },
        {3, 10f },
        {4, 4f },
        {5, 1.5f },
        {6, 0.7f },
        {7, 0.3f },
        {8, 0.1f }
    };

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Slow, but that doesn't really matter in this case

        Initialize();
        GenerateMesh();

        StartCoroutine(PlanetGenerationLoop());
    }

    /* Only update the planet once per second
    Other possible improvements include:
    1: Only updating once the player has moved far enough to be able to cause a noticable change in the LOD
    2: Only displaying chunks that are in sight
    3: Not recreating chunks that already exist */
    private IEnumerator PlanetGenerationLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            GenerateMesh();
        }
    }

    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for(int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, 4, directions[i], size);
        }
    }

    // Generates the mesh. The generation is done from scratch every time it's called, which could be improved
    void GenerateMesh()
    {
        foreach(TerrainFace face in terrainFaces)
        {
            face.ConstructTree();
        }
    }
}   
