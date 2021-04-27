using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.StaticEntity {
	public class Portal: Entity, IInteractiveEntity {
		private Animator _animator;
		private int _nbOfPlayers;
		
		private void Start() {
			_nbOfPlayers = 0;
			_animator = GetComponent<Animator>();
			Instantiate();
		}
		protected override void OnTriggerEnter2D(Collider2D other) {
			base.OnTriggerEnter2D(other);
			if (!isServer) return;
			_nbOfPlayers++;
			CmdOpen();
		}

		protected override void OnTriggerExit2D(Collider2D other) {
			base.OnTriggerExit2D(other);
			if (!isServer) return;
			_nbOfPlayers--;
			CmdClose();
		}

		[Command(requiresAuthority = false)]
		private void CmdOpen() {
			Opening();
		}

		[Command(requiresAuthority = false)]
		private void CmdClose() {
			Closing();
		}

		[ClientRpc]
		private void Opening() {
			if (_nbOfPlayers != 1) return;
			_animator.Play("Opening");
			_animator.SetBool("IsPlayerHere", true);
		}
	
		[ClientRpc]
		private void Closing() {
			if (_nbOfPlayers != 0) return;
			_animator.SetBool("IsPlayerHere", false);
		}

		[Server]
		public void Interact(Player player) {
			Debug.Log("salut les salopes");
		}
	}
}
