using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderManagement : MonoBehaviour
{
    public Camera close;
    public Camera far;

    void Start() {

        far.SetReplacementShader(Shader.Find("Planet/PlanetFar"), "Planet");
        
    }
}
