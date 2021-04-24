using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace SwitchLevels {
	public class TriggerFromTo: MonoBehaviour {
		[Header("Sound Settings")]
		[SerializeField] private AudioDB audioDB;

		[Header("To destination")]
		[SerializeField] private string sceneToGo;
		[SerializeField] private string musicToPlay;

		private void OnTriggerEnter2D(Collider2D other) {
			// Verifying the collider is a player
			if (other.gameObject.GetComponent<Player>() == null) return;
			NetworkManager.singleton.ServerChangeScene(sceneToGo);
			LocalGameManager.Instance.audioManager.PlayMusic(musicToPlay);
			Debug.Log("Changed scene !");
		}
	}
}
