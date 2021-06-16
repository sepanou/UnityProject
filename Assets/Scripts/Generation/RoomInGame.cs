using System;
using System.Collections;
using System.Collections.Generic;
using DataBanks;
using Entity.DynamicEntity.LivingEntity;
using Entity.DynamicEntity.LivingEntity.Mob;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
            if (!hasBeenCleared) {
                AudioDB.PlayUISound("treesDoorsLowering");
                StartCoroutine(StartRaiseWalls());
                if (isServer) CustomNetworkManager.Instance.PlayerPrefabs.ForEach(player => player.Orchid++);
            }
        }
        
        [SyncVar(hook = nameof(SyncHasBeenClearedChanged))] public bool hasBeenCleared;
        private void SyncHasBeenClearedChanged(bool oldHbc, bool hbc) {
            if (!hbc) return;
            AudioDB.PlayUISound("treesDoorsRaising");
            Destroy(doorsColliders);
            StartCoroutine(StartLowerWalls());
        }

        [SerializeField] private GameObject graphicsGo;
        [SerializeField] private SpriteRenderer cover;
        [SerializeField] private Collider2D triggerZone;
        [SerializeField] private GameObject doorsColliders;
        [SerializeField] private GameObject doorsTrees;
        [SerializeField] private SpawnObjects[] prefabsToSpawn;
        [SerializeField] private MobsPrefabsDB mobsAvailable;
        [SerializeField] private GameObject[] mobsSpawnGO;
        private Vector3[] _mobSpawns;
        private List<Mob> _mobs;
        private InputManager _inputManager;

        private void Start() {
            // Change trees and change walls
            doorsTrees.transform.localScale = new Vector3(1, 1, 0);
            foreach (Transform child in doorsTrees.transform)
                child.localScale = new Vector3(1, 0, 0);
            _mobSpawns = new Vector3[mobsSpawnGO.Length];
            for (int i = 0; i < _mobSpawns.Length; i++) {
                _mobSpawns[i] = mobsSpawnGO[i].transform.position;
            }

            _mobs = new List<Mob>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            graphicsGo.SetActive(true);
            foreach ((GameObject pos, GameObject toSpawn) in prefabsToSpawn)
                NetworkServer.Spawn(Instantiate(toSpawn, pos.transform.position, Quaternion.identity));
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
                yield return new WaitForSeconds(0.016f);
            }
        }

        [Client] private IEnumerator StartLowerWalls() {
            float count = 1;
            while (count > 0f) {
                count -= 0.00833f;
                foreach (Transform child in doorsTrees.transform) {
                    child.localScale = new Vector3(1, count, 0);
                }
                yield return new WaitForSeconds(0.016f);
            }
            Destroy(doorsTrees);
        }

        [Client] private void HideCover() {
            if (cover) {
                cover.color = new Color(255, 255, 255, 0);
                if (cover.TryGetComponent(out Collider2D box))
                    Destroy(box);
                if (cover.TryGetComponent(out GameObject gO))
                    Destroy(gO);
            }
            if (triggerZone) Destroy(triggerZone);
        }

        [Server] private void SpawnMobs() {
            bool placedOne = false;
            foreach (Vector3 pos in _mobSpawns) {
                if (!(!placedOne || Random.Range(0, 100) <= 90)) continue;
                placedOne = true;
                GameObject mobGO = mobsAvailable.mobsPrefabs[Random.Range(0, mobsAvailable.mobsPrefabs.Length)];
                if (!Instantiate(mobGO, pos, quaternion.identity).TryGetComponent(out Mob mob)) return;
                mob.OnEntityDie.AddListener(CheckRoomCleared);
                _mobs.Add(mob);
                NetworkServer.Spawn(mob.gameObject);
            }
        }

        private IEnumerator VisibilityChecker() {
            Player localPlayer;
            while ((localPlayer = LocalGameManager.Instance.LocalPlayer) != null && localPlayer && cover) {
                graphicsGo.SetActive(localPlayer.IsSpriteVisible(cover));
                yield return new WaitForSeconds(LocalGameManager.Instance.visibilityUpdateDelay);
            }
        }

        [Server] private void CheckRoomCleared(LivingEntity entity) {
            if (hasBeenCleared || !hasBeenDiscovered) return;
            _mobs.Remove(entity as Mob);
            hasBeenCleared = _mobs.Count == 0;
        } 

        [ServerCallback] private void Update() {
            if (hasBeenCleared && Input.GetKeyDown(KeyCode.T))
                FindObjectOfType<BossRoom>().GenerateStuffAndTp();
            // Kills EVERYTHING :)
            if (!Input.GetKeyDown(KeyCode.P) || !hasBeenDiscovered) return;
            // Don't change to foreach or whatever otherwise, _mobs while be modified during the loop :/
            for (int i = 0; i < _mobs.Count; i = 0)
                _mobs[i].GetAttacked(int.MaxValue);
        }
        
        [ServerCallback] private void OnTriggerEnter2D(Collider2D other) {
            if (hasBeenDiscovered) return;
            
            if (!CustomNetworkManager.Instance.AlivePlayers.TrueForAll(player =>
                triggerZone.IsTouching(player.Collider2D)))
                return;
            hasBeenDiscovered = true;
            if (cover.TryGetComponent(out Collider2D box))
                Destroy(box);
            Destroy(triggerZone);

            if (hasBeenCleared) return;
            foreach (Collider2D col in doorsColliders.GetComponents<Collider2D>())
                col.isTrigger = false;
            SpawnMobs();
        }
    }
}