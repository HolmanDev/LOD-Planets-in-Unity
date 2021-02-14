using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 1f;
    public float rSpeed = 1f;

    int right;
    int left;
    int up;
    int down;
    int forward;
    int backward;

    private void Update()
    {
        if(Input.GetKey(KeyCode.D))
        {
            right = 1;
        } 
        else
        {
            right = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            left = 1;
        }
        else
        {
            left = 0;
        }

        if (Input.GetKey(KeyCode.E))
        {
            up = 1;
        }
        else
        {
            up = 0;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            down = 1;
        }
        else
        {
            down = 0;
        }

        if (Input.GetKey(KeyCode.W))
        {
            forward = 1;
        }
        else
        {
            forward = 0;
        }

        if (Input.GetKey(KeyCode.S))
        {
            backward = 1;
        }
        else
        {
            backward = 0;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(rSpeed,0,0) * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(new Vector3(-rSpeed, 0, 0) * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(new Vector3(0, rSpeed, 0) * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -rSpeed, 0) * Time.deltaTime);
        }

        if(Input.GetKey(KeyCode.LeftShift)) {
            speed += 50000 * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.LeftControl)) {
            speed -= 50000 * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        transform.position += (transform.right * (right - left) + transform.up * (up - down) + transform.forward * (forward - backward)) * speed * Time.deltaTime;
    }
}
