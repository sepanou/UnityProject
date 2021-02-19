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

    // Start is called before the first frame update
    void Start()
    {
        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            animator.SetBool("isAttacking", true);
            isAttacking = true;
        }
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
