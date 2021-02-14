using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// A lot of this code comes from Sebastian Lague, credit goes to him
[System.Serializable]
public class NoiseFilter
{
    public Noise noise = new Noise();
    public Vector3 center;
    
    public bool absoluteEnabled = false;
    public bool invertEnabled = false;

    public float strength = 1;
    [Range(1, 8)] public int octaves = 1;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistance = .5f;
    public int power = 1;

    public bool clampingEnabled = false;
    public float minValue = 0;
    public float maxValue = 1;
    public float multiplier = 1;

    public bool turbulanceEnabled = false;
    public float turbulance = 0;
    public float turbulanceSize = 1;

    public bool positive = false;

    // Get a noise value from a specific point in a 3D simplex noise
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        if(turbulanceEnabled) {
            float turbulanceAmount = turbulance * noise.Evaluate(point * turbulanceSize);
            point.x = point.x + turbulanceAmount;
            point.y = point.y + turbulanceAmount;
        }

        for (int i = 0; i < octaves; i++)
        {
            float v = noise.Evaluate(point * frequency + center);
            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        if(clampingEnabled) {
            noiseValue = Mathf.Clamp(noiseValue * multiplier, minValue, maxValue);
        }

        if(absoluteEnabled) {
            noiseValue = Mathf.Abs(noiseValue);
        }

        if(positive) {
            noiseValue = (noiseValue + 1) / 2f;
        }

        if(invertEnabled) {
            noiseValue = 1f - noiseValue;
        }

        if(power <= 1) return noiseValue * strength;
        if(power == 2) return noiseValue * noiseValue * strength;
        if(power == 3) return noiseValue * noiseValue * noiseValue * strength;
        if(power == 4) return noiseValue * noiseValue * noiseValue * noiseValue * strength;
        if(power == 5) return noiseValue * noiseValue * noiseValue * noiseValue * noiseValue * strength;
        
        return Mathf.Pow(noiseValue, power);
    }

    public double EvaluateD(Vector3 point)
    {
        double noiseValue = 0;
        float frequency = 270f;
        double amplitude = 1;
        double p = 0.3;
        double s = 0.00015;

        if(turbulanceEnabled) {
            float turbulanceAmount = turbulance * noise.Evaluate(point * turbulanceSize);
            point.x = point.x + turbulanceAmount;
            point.y = point.y + turbulanceAmount;
        }

        for (int i = 0; i < octaves; i++)
        {
            double v = noise.Evaluate(point * frequency + center);
            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= p;
        }

        if(clampingEnabled) {
            noiseValue = Math.Min((Math.Max(noiseValue * (double) multiplier, (double) minValue)), (double) maxValue);
        }

        if(absoluteEnabled) {
            noiseValue = Math.Abs(noiseValue);
        }

        if(positive) {
            noiseValue = (noiseValue + 1) / 2.0;
        }

        if(invertEnabled) {
            noiseValue = 1.0 - noiseValue;
        }

        if(power <= 1) return noiseValue * s;
        if(power == 2) return noiseValue * noiseValue * s;
        if(power == 3) return noiseValue * noiseValue * noiseValue * s;
        if(power == 4) return noiseValue * noiseValue * noiseValue * noiseValue * s;
        if(power == 5) return noiseValue * noiseValue * noiseValue * noiseValue * noiseValue * s;
        
        return Math.Pow(noiseValue, power);
    }
}