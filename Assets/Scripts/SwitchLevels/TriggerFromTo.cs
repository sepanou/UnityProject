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
		[SerializeField] private Vector2 whereToSpawn;

		private void OnTriggerEnter2D(Collider2D other) {
			// Verifying the collider is a player
			if (other.gameObject.GetComponent<Player>() == null) return;
			NetworkManager.singleton.ServerChangeScene(sceneToGo);
			AudioDB.PlayMusic(musicToPlay);
			other.gameObject.transform.position = whereToSpawn;
			Debug.Log("Changed scene !");
		}
	}
}
