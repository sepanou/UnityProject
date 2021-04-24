using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace SwitchLevels
{
    public class TriggerFromTo : MonoBehaviour
    {
        [Header("Sound Settings")]
        [SerializeField] private AudioDB audioDB;

        [Header("To destination")]
        [SerializeField] private string sceneToGo;
        [SerializeField] private string musicToPlay;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Player tmp = other.gameObject.GetComponent<Player>(); // Verifying the collider is a player
            if (tmp == null) return;
            NetworkManager.singleton.ServerChangeScene(sceneToGo);
            LocalGameManager.Instance.audioManager.PlayMusic(musicToPlay);
            Debug.Log("Changed scene !");
        }
    }
}
