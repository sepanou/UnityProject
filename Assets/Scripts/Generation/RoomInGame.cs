using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;
using Object = System.Object;

namespace Generation{
    public class RoomInGame: NetworkBehaviour {
        
        [Serializable]
        private class SpawnObjects{
            public GameObject spawnPos;
            public GameObject prefabToSpawn;

            public void Deconstruct(out GameObject o, out GameObject gameObject1) {
                o = spawnPos;
                gameObject1 = prefabToSpawn;
            }
        }
        public bool hasBeenDiscovered;
        [SyncVar(hook = nameof(SyncHasBeenClearedChanged))] public bool hasBeenCleared;

        [SerializeField] private GameObject graphicsGo;
        [SerializeField] private SpriteRenderer cover;
        [SerializeField] private Collider2D triggerZone;
        [SerializeField] private GameObject doorsColliders;
        [SerializeField] private GameObject doorsTrees;
        [SerializeField] private SpawnObjects[] prefabsToSpawn;
        private InputManager _inputManager;

        [Server]
        private void Start() {
            graphicsGo.SetActive(true);
            RpcChangeWalls(1);
            RpcChangeTrees(0);
            foreach ((GameObject pos, GameObject toSpawn) in prefabsToSpawn) 
                NetworkServer.Spawn(Instantiate(toSpawn, pos.transform.position, Quaternion.identity));
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
            RpcRaiseWalls();
        }

        [ClientRpc]
        private void RpcChangeWalls(int scale) {
            doorsTrees.transform.localScale = new Vector3(1, scale, 0);
        }

        [ClientRpc]
        private void RpcChangeTrees(int scale) {
            foreach (Transform child in doorsTrees.transform) {
                child.localScale = new Vector3(1, scale, 0);
            }
        }

        [ClientRpc]
        private void RpcRaiseWalls() {
            StartCoroutine(StartRaiseWalls());
        }
        private IEnumerator StartRaiseWalls() {
            float count = 0;
            while (count < 1f) {
                count += 0.00833f;
                foreach (Transform child in doorsTrees.transform) {
                    child.localScale = new Vector3(1, count, 0);
                }
                yield return new WaitForSeconds(0.004f);
            }
        }
        
        [ClientRpc]
        private void RpcLowerWalls() {
            StartCoroutine(StartLowerWalls());
        }
        
        private IEnumerator StartLowerWalls() {
            float count = 1;
            while (count > 0f) {
                count -= 0.00833f;
                foreach (Transform child in doorsTrees.transform) {
                    child.localScale = new Vector3(1, count, 0);
                }
                yield return new WaitForSeconds(0.004f);
            }
            Destroy(doorsTrees);
        }

        private void SetGlobalScale(Transform transformObj, Vector3 globalScale) {
            transformObj.localScale = Vector3.one;
            transformObj.localScale = new Vector3 (1, globalScale.y/transformObj.lossyScale.y, 0);
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
                RpcLowerWalls();
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