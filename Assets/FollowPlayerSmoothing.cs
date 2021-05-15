using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerSmoothing : MonoBehaviour
{
    public Transform player;
    public float smoothing;
    public Vector3 offset; 

    private void Update()
    {
        transform.position = player.position + offset; 
    }
}
