using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UI_Audio;
using Entity.DynamicEntity.LivingEntity.Player;

public class HubToForest : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioDB audioDB;

    [Header("To destination")]
    [SerializeField] private string sceneToGo;
    [SerializeField] private string musicToPlay;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("YEEEEEEET");
        Player tmp = other.gameObject.GetComponent<Player>(); // Verifying the collider is a player
        if (tmp == null) return;
        NetworkManager.singleton.ServerChangeScene(sceneToGo);
        audioDB.PlayMusic(musicToPlay);
        Debug.Log("Changed scene !");
    }
}
