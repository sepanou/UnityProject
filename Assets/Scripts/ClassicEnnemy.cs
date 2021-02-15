using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClassicEnnemy : MonoBehaviour
{
    public Quaternion transformRotation;
    public Vector2 lastPos;
    public Animator animator;
    public GameObject player;
    public Rigidbody2D slimebody;
    public int x;

    public bool isBoucing;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 lastPos = transform.position;
        isBoucing = false;
        int x = 1;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        isBoucing = true;
        int x = -1;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        WalkingAnimation();
        Vector2 move = Vector2.MoveTowards(transform.position, player.transform.position, .03f);
        ;
        if (isBoucing)
        {
            transform.position = lastPos;
            isBoucing = false;

        }
        else
        {
            transform.position = move;
        }
    }

    void WalkingTo()
    {
        
    }

    void WalkingAnimation()
    {
        transformRotation = transform.rotation;
        Vector2 displacement = (Vector2) transform.position - lastPos;
        if (transform.position.x < lastPos.x)
        {
            transformRotation.y = 0f;
        }
        else
        {
            transformRotation.y = 180f;
        }
        lastPos = transform.position;
        transform.rotation = transformRotation;
        animator.SetBool("IsWalking", (displacement.magnitude > 0.00001));
    }
}
