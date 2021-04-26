using System;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public class Portal : Entity.Entity, IInteractiveEntity
{
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        Instantiate();
    }
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (!isServer) return;
        Opening();
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (!isServer) return;
        Closing();
    }

    [ClientRpc]
    private void Opening()
    {
        _animator.Play("Opening");
        _animator.SetBool("IsPlayerHere", true);
    }
    
    [ClientRpc]
    private void Closing()
    {
        _animator.SetBool("IsPlayerHere", false);
    }

    public void CmdInteract(Player player)
    {
        Debug.Log("salut les salopes");
    }
}
