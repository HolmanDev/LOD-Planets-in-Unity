using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyExtensions
{
    /// <summary>
    /// Get the world velocity of a point on the rigid body. This includes torque. 
    /// Default space is self.
    /// </summary>
    public static Vector3 GetVelocityAtPoint(this Rigidbody rb, Vector3 point, Space space = Space.Self)
    {
        Vector3 pointVelocity = rb.velocity; // Velocity of center of mass
        Vector3 angularVelocity = rb.angularVelocity; // Angular velocity of center of mass

        if (space == Space.World) {

            point = rb.transform.InverseTransformPoint(point);
        }

        // v = w × r
        Vector3 tangentialVelocity = Vector3.Cross(angularVelocity, rb.transform.TransformDirection(point - rb.centerOfMass));
        pointVelocity += tangentialVelocity;

        return pointVelocity;
    }
}
