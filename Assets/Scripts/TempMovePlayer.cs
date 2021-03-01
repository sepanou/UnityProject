using UnityEngine;
using Mirror;

public class TempMovePlayer: NetworkBehaviour {
    private Rigidbody2D _body;

    public Camera _playerCamera;
    
    [SerializeField] private float velocity;
    
    void Start() {
        _body = GetComponent<Rigidbody2D>();
        
        _playerCamera.gameObject.SetActive(isLocalPlayer);
    }
    
    void FixedUpdate() {
        if (isLocalPlayer) {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);
            direction.Normalize();
            _body.velocity = velocity * direction;
        }
    }
}