using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagement : MonoBehaviour
{
    public Transform[] objectsToMove;
    public Transform player;
    public float maxTravelDist = 100f;
    public Planet planetScript;

    private void Update()
    {
        if(Mathf.Abs(player.position.magnitude) > 100f)
        {
            for(int i = 0; i < objectsToMove.Length; i++)
            {
                objectsToMove[i].position -= player.position;
            }
            //for(int i = 0; i < planetScript.lastRecordedPlayerPos.Length; i++)
            //{
                //planetScript.lastRecordedPlayerPos[i] -= player.position;
            //}

            player.position = Vector3.zero;
        }
    }
}
