using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private bool isFollowing;

    public float followSpeed;
    public Transform followTarget;
    public GameObject key;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if (isFollowing)
        // {
        //     transform.position = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
        // }
        
     
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!isFollowing)
            {
                PlayerMovement player = FindObjectOfType<PlayerMovement>();
                // followTarget = player.keyFollowPoint;
                isFollowing = true;
                player.isPicked = true;
                key.SetActive(false);
                Destroy(this);
            }
        }
    }
}