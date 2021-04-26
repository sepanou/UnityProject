using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace SwitchLevels {
	public class TriggerFromTo: NetworkBehaviour { 
		[Header("To destination")]
		[SerializeField] private string sceneToGo;


		private void OnTriggerEnter2D(Collider2D other) {
			// Verifying the collider is a player
			if (other.gameObject.GetComponent<Player>() == null) return;
			Switch();
		}

		[Command(requiresAuthority = false)]
		private void Switch() => NetworkManager.singleton.ServerChangeScene(sceneToGo);
	}
}
