using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    private bool isAttacking;

    private Quaternion transformRotation;
    // Start is called before the first frame update
    void Start()
    {
        isAttacking = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
        }
        if (isAttacking)
        {
            transformRotation = transform.rotation;
            transformRotation.z -= 0.05f;
            if (transformRotation.z <= -90f)
            {
                transformRotation.z = 90f;
                isAttacking = false;
                Debug.Log("époiefeo");
            }

            transform.rotation = transformRotation;
        }
    }
}
