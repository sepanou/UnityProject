using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicEnnemy : MonoBehaviour
{
    public Quaternion transformRotation;
    public Vector3 lastPos;
    public Animator animator;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (HasMoved())
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, .03f);
    }

    bool HasMoved()
    {
        transformRotation = transform.rotation;
        Vector3 displacement = transform.position - lastPos;
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
        return displacement.magnitude > 0.00001;
    }
}
