using UnityEngine;
using Mirror;

public class TempMovePlayer: NetworkBehaviour {
	private Rigidbody2D _body;
	public Camera playerCamera;
	
	[SerializeField]
	private float velocity;
	
	private void Start() {
		_body = GetComponent<Rigidbody2D>();
		playerCamera.gameObject.SetActive(isLocalPlayer);
	}
	
	private void FixedUpdate() {
		if (isLocalPlayer) {
			float x = Input.GetAxisRaw("Horizontal");
			float y = Input.GetAxisRaw("Vertical");
			Vector2 direction = new Vector2(x, y);
			direction.Normalize();
			if (direction.sqrMagnitude != 0)
				_body.velocity = Vector2.zero;
			_body.position += velocity * direction;
		}
	}
}