using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    [SerializeField] private Transform _target = default;
    [SerializeField] private float _distance = 100;
    [SerializeField] private Vector3 _rotation = Vector3.up;
    [SerializeField] private bool _lookAtTarget = false;

    private void LateUpdate() {
        transform.position += Vector3.Cross(_rotation, _target.position - transform.position).normalized * _rotation.magnitude * Time.deltaTime;
        transform.position += (_target.position - transform.position).normalized * ((_target.position - transform.position).magnitude  - _distance);
        if(_lookAtTarget) {
            transform.LookAt(_target);
        }
    }
}
