using System;
using System.Collections;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

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
        
        [SyncVar(hook = nameof(SyncHasBeenDiscoveredChanged))] public bool hasBeenDiscovered;
        private void SyncHasBeenDiscoveredChanged(bool oldHbd, bool hbd) {
            if (!hbd) return;
            HideCover();
            if (!hasBeenCleared) StartCoroutine(StartRaiseWalls());
        }
        
        [SyncVar(hook = nameof(SyncHasBeenClearedChanged))] public bool hasBeenCleared;
        private void SyncHasBeenClearedChanged(bool oldHbc, bool hbc) {
            if (!hbc) return;
            Destroy(doorsColliders);
            StartCoroutine(StartLowerWalls());
        }

        [SerializeField] private GameObject graphicsGo;
        [SerializeField] private SpriteRenderer cover;
        [SerializeField] private Collider2D triggerZone;
        [SerializeField] private GameObject doorsColliders;
        [SerializeField] private GameObject doorsTrees;
        [SerializeField] private SpawnObjects[] prefabsToSpawn;
        private InputManager _inputManager;

        private void Start() {
            // Change trees and change walls
            doorsTrees.transform.localScale = new Vector3(1, 1, 0);
            foreach (Transform child in doorsTrees.transform)
                child.localScale = new Vector3(1, 0, 0);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            graphicsGo.SetActive(true);
            foreach ((GameObject pos, GameObject toSpawn) in prefabsToSpawn) {
                GameObject obj = Instantiate(toSpawn, pos.transform.position, Quaternion.identity);
                obj.transform.SetParent(pos.transform);
                NetworkServer.Spawn(obj);
            }
        }

        public override void OnStartClient() {
            base.OnStartClient();
            StartCoroutine(VisibilityChecker());
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState) {
            base.OnSerialize(writer, initialState);
            writer.WriteBoolean(hasBeenCleared);
            writer.WriteBoolean(hasBeenDiscovered);
            return true;
        }
        
        public override void OnDeserialize(NetworkReader reader, bool initialState) {
            base.OnDeserialize(reader, initialState);
            if (reader.ReadBoolean() != hasBeenCleared) {
                SyncHasBeenClearedChanged(hasBeenCleared, !hasBeenCleared);
                hasBeenCleared = !hasBeenCleared;
            }

            if (reader.ReadBoolean() == hasBeenDiscovered) return;
            SyncHasBeenDiscoveredChanged(hasBeenDiscovered, !hasBeenDiscovered);
            hasBeenDiscovered = !hasBeenDiscovered;
        }

        [Client] private IEnumerator StartRaiseWalls() {
            float count = 0;
            while (count < 1f) {
                count += 0.00833f;
                foreach (Transform child in doorsTrees.transform) {
                    child.localScale = new Vector3(1, count, 0);
                }
                yield return new WaitForSeconds(0.004f);
            }
        }

        [Client] private IEnumerator StartLowerWalls() {
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

        [Client] private void HideCover() {
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

        [ServerCallback] private void Update() {
            if (hasBeenCleared || !Input.GetKeyDown(KeyCode.P) || !hasBeenDiscovered) return;
            hasBeenCleared = true;
            CustomNetworkManager.Instance.PlayerPrefabs.ForEach(player => player.Orchid++);
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

            if (hasBeenCleared) return;
            foreach (Collider2D col in doorsColliders.GetComponents<Collider2D>())
                col.isTrigger = false;
        }
    }
}