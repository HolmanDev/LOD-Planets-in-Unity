using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    //[SerializeField] Vector3 _gravity = default;
    //public Vector3 Gravity => _gravity;

    [Header("General")]
    public static GameManagement instance;
    public Transform player;
    public Transform cam;

    [Header("Planet")]
    public bool onPlanet = true; // Enable when in the universe with planet and stuff, disable when testing.
    public Transform[] objectsToMove;
    public float maxTravelDist = 100f;
    public Planet planetScript;
    public Transform Universe;
    public static float farShrinkFactor = 100000f;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }
    }

    private void FixedUpdate()
    {
        if (onPlanet)
        {
            UpdatePos();
        }
    }

    private void UpdatePos() {
        if (Mathf.Abs(player.position.magnitude) > maxTravelDist)
        {
            Vector3 offset = player.position;

            Universe.position -= offset;
            player.position = Vector3.zero;
        }
    }
}