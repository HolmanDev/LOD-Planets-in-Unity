using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class Planet : MonoBehaviour
{
    [HideInInspector] public CachedPlanet CachedPlanet;
    [HideInInspector] public MeshFilter[] meshFilters;
    [HideInInspector] public TerrainFace[] terrainFaces;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Transform player;
    [HideInInspector] public Vector3 PlayerPos;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToPlayerPow2;
    public int vertexColorMinLOD = 4;

    public float Size = 1000000;
    public bool CullingEnabled = true;
    public float CullingMinAngle = 1.45f;
    public Material SurfaceMat;
    //public PlanetCollider planetCollider;
    public TerrainConfig LowDefElevationConfig;
    public TerrainConfig HighDefElevationConfig;
    public BiomeConfig BiomeConfig;
    #region IMPORTANT
     /* The planet cannot support an infinite number of vertices. If your planet doesn't render, consider the follow:
     1. Make the planet less detailed where the player doesn't notice. The beauty with an LOD planet is that it allows for increasingly 
        detailed terrain around the player. It is wise to utilize this.
     2. Change the size of the planet. If you've begun working on a planet with a million-unit diameter and then proceed to shrink it, 
        the detail levels distances will be too large, causing the entire planet to become extremely detailed, but slow (and invisible). */
     #endregion
    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        Mathf.Infinity,
        Mathf.Infinity,
        450000,
        200000,
        100000,
        50000,
        30000,
        16000,
        8000,
        4000,
        2300,
        1400,
        750,
        500,
        300
    };

    private int printed;

    // Multithreading
    public List<Action> ActionQueue = new List<Action>(); // Use Queue datatype instead of List?
    public object _asyncLock = new object();

    public void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Slow, but that doesn't really matter in this case
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    public void Start()
    {
        if(terrainFaces == null || terrainFaces.Length == 0)
        {
            Initialize();
            LoadCachedPlanet(CachedPlanet);
        }
        GenerateMesh(false); // Is this needed?
        UpdateShaders(); // Is this needed?
        StartCoroutine(PlanetGenerationLoop());
    }

    public void Update()
    {
        ExecuteActionQueue();
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
        position = transform.position;
        PlayerPos = player.transform.position;
    }

    /// <summary>
    /// Executes the actions queued by other threads on the main thread.
    /// </summary>
    private void ExecuteActionQueue()
    {
        if (ActionQueue != null)
        {
            lock (_asyncLock)
            {
                List<Action> actionsToDelete = new List<Action>();
                List<Action> actionQueue = new List<Action>(ActionQueue);
                foreach (Action action in actionQueue)
                {
                    if (action != null)
                    {
                        action.Invoke();
                        actionsToDelete.Add(action);
                    }
                }
                foreach (Action action in actionsToDelete)
                {
                    if (action != null)
                    {
                        actionQueue.Remove(action);
                    }
                }
                ActionQueue = actionQueue;
            }
        }
    }

    // Only update the planet once per second
    private IEnumerator PlanetGenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            GenerateMesh(true);
            UpdateShaders();
        }
    }

    /// <summary>
    /// Executes the actions queued by other threads on the main thread. Credit to Sebastian Lague for much of the code.
    /// </summary>
    public void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        // Connect existing faces to the mesh filters (This is a code smell)
        Transform[] faces = transform.GetChildrenWithTag("PlanetFace");
        for(int i = 0; i < faces.Length; i++) {
            meshFilters[i] = faces[i].GetComponent<MeshFilter>();
        }

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                // Create new game objects
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;
                meshObject.transform.position = transform.position;
                meshObject.tag = "PlanetFace";
                meshObject.layer = LayerMask.NameToLayer("Planet");

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(SurfaceMat);
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Allow more vertices
            } else {
                // Update old game objects
                if(meshFilters[i].GetComponent<MeshRenderer>() == null) {
                    meshFilters[i].gameObject.AddComponent<MeshRenderer>();
                }
                meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(SurfaceMat);
                meshFilters[i].gameObject.layer = LayerMask.NameToLayer("Planet");
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], Size, this);
        }
    }

    public void GenerateMesh(bool multithread = false)
    {
        foreach (TerrainFace face in terrainFaces)
        {
            if(multithread) {
                Thread t = new Thread(() => { 
                    face.GenerateTree(true);
                });
                t.Start();
            } else {
                face.GenerateTree();
            }
        }
    }

    public void GenerateTexture() {
        foreach (TerrainFace face in terrainFaces)
        {
            Color[] data = face.GenerateTextureData(face.textureWidth, face.textureBorder);
            face.SetTexture(face.textureWidth, face.textureBorder, data); 
        }
    }

    /// <summary>
    /// Update the shaders, i.e by assigning textures.
    /// </summary>
    public void UpdateShaders() {
        for(int i = 0; i < meshFilters.Length; i++) {
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", terrainFaces[i].texture);
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_ShrinkFactor", GameManagement.farShrinkFactor);
        }
    }

    /// <summary>
    /// Store the mesh and texture.
    /// </summary>
    public CachedPlanet CachePlanet () {
        CachedFace[] cachedFaces = new CachedFace[6];
        for(int i = 0; i < 6; i++) {
            cachedFaces[i] = new CachedFace(terrainFaces[i].mesh, terrainFaces[i].texture);
        }
        return new CachedPlanet(cachedFaces);
    }

    /// <summary>
    /// Load the mesh and texture.
    /// </summary>
    private void LoadCachedPlanet(CachedPlanet cachedPlanet)
    {
        for (int i = 0; i < 6; i++)
        {
            if (cachedPlanet != null)
            {
                terrainFaces[i].mesh = cachedPlanet.cachedFaces[i].mesh;
                terrainFaces[i].texture = cachedPlanet.cachedFaces[i].texture;
            }
        }
    }
}