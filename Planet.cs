using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Most of the code in this script is made by Sebastian Lague, except for the LOD stuff
public class Planet : MonoBehaviour
{
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public float cullingMinAngle = 1.91986218f;
    public float size = 1000; // Must be set to the size of the planet defined in the inspector

    public Transform player;
    public float distanceToPlayer;

    // Hardcoded detail levels. First value is level, second is distance from player. Finding the right values can be a little tricky
    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        6000f,
        2500f,
        1000f,
        400f,
        150f,
        70f,
        30f,
        10f
    };

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Slow, but that doesn't really matter in this case

        Initialize();
        GenerateMesh();

        StartCoroutine(PlanetGenerationLoop());
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    // Only update the planet once per second
    private IEnumerator PlanetGenerationLoop()
    {
        GenerateMesh();

        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateMesh();
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

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], size, this);
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

    // Update the mesh.
    void UpdateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.UpdateTree();
        }
    }
}   
