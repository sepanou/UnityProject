using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UI_Audio;

public class HubToForest : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioDB audioDB;
    private void OnTriggerEnter(Collider other)
    {
        NetworkManager.singleton.ServerChangeScene("Level1Creation");
        audioDB.PlayMusic("ForestMusic");
        Debug.Log("Changed scene !");
    }
}
