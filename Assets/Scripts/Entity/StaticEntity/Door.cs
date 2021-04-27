using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.StaticEntity {
	public class Door: Entity, IInteractiveEntity {
		private Collider2D[] _doorCollider;
		private SpriteRenderer _spriteRenderer;

		[SerializeField] private bool isOpen;
		[SerializeField] private Sprite closed;
		[SerializeField] private Sprite opened;


		private void Start() {
			_doorCollider = GetComponents<Collider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			Instantiate();
			AutoStopInteracting = true;
		}

		[ClientRpc]
		private void RpcToggleSprite(bool isOpen2) {
			AudioDB.PlayUISound("WoodenDoor");
			_spriteRenderer.sprite = isOpen2 ? closed : opened;
		}

		[Command(requiresAuthority = false)] 
		public void CmdInteract(Player player) {
			_doorCollider[0].enabled = isOpen;
			RpcToggleSprite(isOpen);
			isOpen = !isOpen;
		}

		public void ToDoOnEntering(Player player){}

		public void ToDoOnExiting(Player player){}
	}
}
