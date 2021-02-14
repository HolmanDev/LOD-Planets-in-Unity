using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarCamera : MonoBehaviour
{
    private Camera cam;
    public Camera closeCamera;
    public Planet planet;
    public Transform universe;

    void LateUpdate() {
        transform.position = closeCamera.transform.position / GameManagement.farShrinkFactor;
        transform.rotation = closeCamera.transform.rotation;
    }
 
     void Start()
     {
         cam = this.GetComponent<Camera>();
     }
 
     void OnPreCull()
     {
        cam.cullingMatrix = Matrix4x4.Ortho(-9999999, 9999999, -9999999, 9999999, 0.01f, 999999999) * 
                             Matrix4x4.Translate(Vector3.forward / 2f) * 
                             cam.worldToCameraMatrix;
        closeCamera.cullingMatrix = Matrix4x4.Ortho(-9999999, 9999999, -9999999, 9999999, 0.01f, 999999999) * 
                             Matrix4x4.Translate(Vector3.forward / 2f) * 
                             cam.worldToCameraMatrix;
     }
 
     void OnDisable()
     {
         cam.ResetCullingMatrix();
     }
}
