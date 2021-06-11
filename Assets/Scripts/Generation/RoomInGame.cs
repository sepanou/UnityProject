using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Generation{
    public class RoomInGame: NetworkBehaviour {
        public bool hasBeenDiscovered;
        public bool hasBeenCleared;
        public Room Room;

        [SerializeField] private GameObject graphicsGo;
        [SerializeField] private SpriteRenderer cover;
        [SerializeField] private Collider2D triggerZone;

        private void Start() {
            hasBeenCleared = false;
            hasBeenDiscovered = false;
            graphicsGo.SetActive(true);
            StartCoroutine(VisibilityChecker());
        }

        [ServerCallback] private void OnTriggerEnter2D(Collider2D other) {
            if (hasBeenDiscovered) return;
            if (!CustomNetworkManager.Instance.PlayerPrefabs.TrueForAll(player =>
                triggerZone.IsTouching(player.Collider2D)))
                return;
            hasBeenDiscovered = true;
            if (cover.TryGetComponent(out Collider2D box))
                Destroy(box);
            Destroy(triggerZone);
            RpcHideCover();
        }

        [ClientRpc] private void RpcHideCover() {
            if (cover) {
                cover.color = new Color(255, 255, 255, 0);
                if (cover.TryGetComponent(out Collider2D box))
                    Destroy(box);
            }
            if (triggerZone) Destroy(triggerZone);
        }

        private IEnumerator VisibilityChecker() {
            Player localPlayer;
            while ((localPlayer = LocalGameManager.Instance.LocalPlayer) != null && localPlayer && cover) {
                graphicsGo.SetActive(localPlayer.IsSpriteVisible(cover));
                yield return new WaitForSeconds(LocalGameManager.Instance.visibilityUpdateDelay);
            }
        }
    }
}