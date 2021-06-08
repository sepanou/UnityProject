using Mirror;
using UnityEngine;

namespace Generation{
    public class RoomInGame: NetworkBehaviour {
        public bool hasBeenDiscovered;
        public bool hasBeenCleared;
        public Room Room;

        [SerializeField] private GameObject cover;
        [SerializeField] private Collider2D triggerZone;

        private void Start() {
            hasBeenCleared = false;
            hasBeenDiscovered = false;
        }
        
        [ServerCallback] private void OnTriggerEnter2D(Collider2D other) {
            if (!CustomNetworkManager.Instance.PlayerPrefabs.TrueForAll(player =>
                triggerZone.IsTouching(player.Collider2D)))
                return;
            hasBeenDiscovered = true;
            Destroy(cover);
            Destroy(triggerZone);
            RpcDestroyCover();
        }

        [ClientRpc] private void RpcDestroyCover() {
            if (cover) Destroy(cover);
            if (triggerZone) Destroy(triggerZone);
        }
    }
}