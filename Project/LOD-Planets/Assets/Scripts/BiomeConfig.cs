using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome Config", menuName = "Planets/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    public Biome[] biomes;
    public Gradient gradient; // A gradient from south to north describing the heat.
    public float distortion;
    public float offset;
    public float noiseScale;
}
