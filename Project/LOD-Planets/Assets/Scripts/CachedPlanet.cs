using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CachedPlanet
{
    public CachedFace[] cachedFaces;

    public CachedPlanet(CachedFace[] cachedFaces) {
        this.cachedFaces = cachedFaces;
    }
}

[System.Serializable]
public class CachedFace 
{
    public Mesh mesh;
    public Texture2D texture;

    public CachedFace(Mesh mesh, Texture2D texture) {
        this.mesh = mesh;
        this.texture = texture;
    }
}
