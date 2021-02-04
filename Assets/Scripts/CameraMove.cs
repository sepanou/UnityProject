using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var positionPlayer = player.transform.position;
        transform.position = new Vector3(positionPlayer.x, positionPlayer.y, transform.position.z);
    }
}
