using Mirror;
using UnityEngine;

public class Door: NetworkBehaviour {
	private Collider2D[] _doorCollider;
	private SpriteRenderer _spriteRenderer;

	[SerializeField] private bool isOpen;
	[SerializeField] private Sprite closed;
	[SerializeField] private Sprite opened;

	private void Start() {
		_doorCollider = GetComponents<Collider2D>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (LocalGameManager.Instance.inputManager.GetKeyDown("Interact"))
			ToggleDoor();
	}
	
	[Command(requiresAuthority = false)]
	private void ToggleDoor() {
		_doorCollider[0].enabled = isOpen;
		ToggleSprite(isOpen);
		isOpen = !isOpen;
	}

	[ClientRpc]
	private void ToggleSprite(bool isOpen2)
	{
		_spriteRenderer.sprite = isOpen2 ? closed : opened;
	}
}
