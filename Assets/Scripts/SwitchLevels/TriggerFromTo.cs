using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace SwitchLevels {
	public class TriggerFromTo: NetworkBehaviour { 
		[Header("To destination")]
		[SerializeField] private string sceneToGo;

		[ClientCallback]
		private void OnTriggerEnter2D(Collider2D other) {
			// Verifying the collider is a player
			if (!other.TryGetComponent(out Player player)) return;
			CustomNetworkManager.Instance.StartSceneTransition();
			if (player.isLocalPlayer)
				Switch();
		}

		[Command(requiresAuthority = false)]
		private void Switch() => NetworkManager.singleton.ServerChangeScene(sceneToGo);
	}
}
