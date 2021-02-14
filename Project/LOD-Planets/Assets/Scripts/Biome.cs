using UnityEngine;

[System.Serializable]
public class Biome {
    public Gradient gradient;
    public float order;

    public Biome(Gradient gradient, float order) {
        this.gradient = gradient;
        this.order = order;
    }
}
