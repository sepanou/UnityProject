using System;
using System.Collections;
using System.Collections.Generic;
using DataBanks;
using UnityEngine;
using UI_Audio;

public class Door : MonoBehaviour
{
    private Collider2D[] doorCollider;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private bool isOpen;
    [SerializeField] private Sprite closed;
    [SerializeField] private Sprite opened;

    private void Start()
    {
        doorCollider = GetComponents<Collider2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("yeet");
        if (LocalGameManager.Instance.inputManager.GetKeyDown("Interact"))
        {
            Debug.Log("Salut");
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        if (isOpen)
        {
            doorCollider[0].enabled = true;
            spriteRenderer.sprite = closed;
            isOpen = false;
        }
        else
        {
            doorCollider[0].enabled = false;
            spriteRenderer.sprite = opened;
            isOpen = true;
        }
    }
}
