using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Config", menuName = "Planets/Terrain Config")]
public class TerrainConfig : ScriptableObject
{
    public NoiseFilter[] NoiseFilters;
}