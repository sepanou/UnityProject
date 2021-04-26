using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace SwitchLevels {
	public class TriggerFromTo: NetworkBehaviour {
		[Header("Sound Settings")]
		[SerializeField] private AudioDB audioDB;

		[Header("To destination")]
		[SerializeField] private string sceneToGo;
		[SerializeField] private string musicToPlay;
		[SerializeField] private Vector2 whereToSpawn;

		
		private void OnTriggerEnter2D(Collider2D other) {
			// Verifying the collider is a player
			if (other.gameObject.GetComponent<Player>() == null) return;
			Switch();
			Debug.Log("Changed scene !");
		}

		[Command(requiresAuthority = false)]
		private void Switch()
		{
			NetworkManager.singleton.ServerChangeScene(sceneToGo);
			SwitchToLevel();
		}
		[ClientRpc]
		private void SwitchToLevel()
		{
			AudioDB.PlayMusic((musicToPlay));
			LocalGameManager.Instance.LocalPlayer.transform.position = whereToSpawn;
		}
	}
}
