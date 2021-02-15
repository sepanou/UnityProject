using UnityEngine;

public class CameraMove: MonoBehaviour {
    public GameObject player;

    void Update() {
        Vector3 positionPlayer = player.transform.position;
        transform.position = new Vector3(positionPlayer.x, positionPlayer.y, transform.position.z);
    }
}
