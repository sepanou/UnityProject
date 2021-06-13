using System;
using System.Collections;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Generation{
    public class RoomInGame: NetworkBehaviour {
        public bool hasBeenDiscovered;
        [SyncVar(hook = nameof(SyncHasBeenClearedChanged))] public bool hasBeenCleared;

        [SerializeField] private GameObject graphicsGo;
        [SerializeField] private SpriteRenderer cover;
        [SerializeField] private Collider2D triggerZone;
        [SerializeField] private GameObject doorsColliders;
        [SerializeField] private GameObject doorsTrees;
        private InputManager _inputManager;

        private void Start() {
            graphicsGo.SetActive(true);
            RpcChangeWalls(0);
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
            if (hasBeenCleared) return;
            foreach (Collider2D col in doorsColliders.GetComponents<Collider2D>()) {
                col.isTrigger = false;
            }
            RpcChangeWalls(1);
        }

        [ClientRpc]
        private void RpcChangeWalls(int scale) {
            doorsTrees.transform.localScale = new Vector3(1, scale, 0);
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

        [Server]
        private void Update() {
            if (!hasBeenCleared && Input.GetKeyDown(KeyCode.P) && hasBeenDiscovered) {
                hasBeenCleared = true;
            }
        }

        private void SyncHasBeenClearedChanged(bool oldHbc, bool hBC) {
            if (hBC) {
                Destroy(doorsColliders);
                Destroy(doorsTrees);
            }
        }
        public override bool OnSerialize(NetworkWriter writer, bool initialState) {
            base.OnSerialize(writer, initialState);
            writer.WriteBoolean(hasBeenCleared);
            return true;
        }
        public override void OnDeserialize(NetworkReader reader, bool initialState) {
            base.OnDeserialize(reader, initialState);
            bool newHbc = reader.ReadBoolean();
            if (newHbc != hasBeenCleared) {
                SyncHasBeenClearedChanged(hasBeenCleared, newHbc);
                hasBeenCleared = newHbc;
            }
        }
    }
}