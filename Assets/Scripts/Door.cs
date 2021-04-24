using UnityEngine;

public class Door: MonoBehaviour {
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

	private void ToggleDoor() {
		_doorCollider[0].enabled = isOpen;
		_spriteRenderer.sprite = isOpen ? closed : opened;
		isOpen = !isOpen;
	}
}
