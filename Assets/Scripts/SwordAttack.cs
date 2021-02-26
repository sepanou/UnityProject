using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Animator animator;
    public bool isAttacking;
    public int atk;
    public bool attacked;
    public Vector3 offset;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        isAttacking = false;
        player = GameObject.Find("Test1");
        offset = player.transform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = player.transform.position - offset;
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            animator.SetBool("isAttacking", true);
            isAttacking = true;
            rotateXdegrees(0f);
        }
    }

    private void OnGUI()
    {
        RotateSwordTowardsMouse();
    }

    private void RotateSwordTowardsMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5.23f - 5.22f;
 
        Vector3 objectPos = Camera.main.WorldToScreenPoint (transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;
 
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void rotateXdegrees(float degrees)
    {
            //
    }
    void finishedAttack() // Used by the animator event
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttacking && other.gameObject.GetComponent<ClassicEnnemy>() is { } ennemy)
        {
            ennemy.TakingDamage(atk);
        }
    }
}
