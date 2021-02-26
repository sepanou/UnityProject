using System;
using UnityEngine;

public class CameraMove: MonoBehaviour {
    private GameObject player;


    private void Start()
    {
        player = GetComponentInParent<GameObject>();
    }

    void FixedUpdate()
    {
        
    }
}
