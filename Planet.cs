using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public float cullingMinAngle = 1.45f;
    public float size = 1000; // Must be set to the size of the planet defined in the inspector

    public Transform player;

    [HideInInspector]
    public float distanceToPlayer;

    [HideInInspector]
    public float distanceToPlayerPow2;

    // First value is level, second is distance from player. Finding the right values can be a little tricky
    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        3000f,
        1100f,
        500f,
        210f,
        100f,
        40f,
    };

    public Material surfaceMat;

    public NoiseFilter noiseFilter;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Slow, but that doesn't really matter in this case
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    private void Start()
    {
        Initialize();
        GenerateMesh();

        StartCoroutine(PlanetGenerationLoop());
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
    }

    // Only update the planet once per second
    private IEnumerator PlanetGenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateMesh();
        }
    }

    // Credit to Sebastian Lague for the following code
    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(surfaceMat);
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], size, this);
        }
    }

    // Generates the mesh
    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructTree();
        }
    }

    // Update the mesh
    void UpdateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.UpdateTree();
        }
    }
}