using System.Collections.Generic;
using DataBanks;
using Entity.DynamicEntity.LivingEntity;
using Entity.DynamicEntity.LivingEntity.Mob;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generation{
	public class BossRoom: NetworkBehaviour {
		[SerializeField] private MobsPrefabsDB mobsAvailable;
		[SerializeField] private GameObject[] mobsSpawnGO;
		[SerializeField] private GameObject bossSpawnPoint;
		[SerializeField] private GameObject bossPrefab;
		[SerializeField] private bool hasBeenTp = false;
		[SerializeField] private bool hasBeenCleared = false;
		private int mobsToSpawn = 20;
		private Vector3[] _mobSpawns;
		private List<Mob> _mobs;
		private InputManager _inputManager;

		[Server]
		private Mob InstantiateRandomMob() {
			Vector3 pos = _mobSpawns[Random.Range(0, _mobSpawns.Length)];
			GameObject mobGO = mobsAvailable.mobsPrefabs[Random.Range(0, mobsAvailable.mobsPrefabs.Length)];
			Instantiate(mobGO, pos, quaternion.identity).TryGetComponent(out Mob mob);
			--mobsToSpawn;
			return mob;
		}

		[Server]
		public void GenerateStuffAndTP() {
			_mobSpawns = new Vector3[mobsSpawnGO.Length];
			for (int i = 0; i < _mobSpawns.Length; i++)
				_mobSpawns[i] = mobsSpawnGO[i].transform.position;
			_mobs = new List<Mob>();
			Instantiate(bossPrefab, bossSpawnPoint.transform.position, quaternion.identity).TryGetComponent(out Mob boss);
			boss.OnEntityDie.AddListener(BossDied);
			NetworkServer.Spawn(boss.gameObject);
			for (int i = 0; i < mobsToSpawn; ++i) {
				Mob mob = InstantiateRandomMob();
				mob.OnEntityDie.AddListener(CheckRoomCleared);
				_mobs.Add(mob);
				NetworkServer.Spawn(mob.gameObject);
			}
			CustomNetworkManager.Instance.AlivePlayers.ForEach(playerToTp => {
				playerToTp.transform.position = new Vector3(11, -7, 0);
			});
			hasBeenTp = true;
		}

		[Server]
		private void CheckRoomCleared(LivingEntity entity) {
			if (!hasBeenTp || hasBeenCleared) return;
			_mobs.Remove(entity as Mob);
			Khrom boss = FindObjectOfType<Khrom>();
			hasBeenCleared = _mobs.Count <= 0 && (!boss || !boss.IsAlive);
		}

		[Server]
		private void BossDied(LivingEntity entity) {
			if (hasBeenCleared) return;
			KillStuff();
		}

		[ServerCallback]
		private void Update() {
			if (!hasBeenTp || hasBeenCleared || !Input.GetKeyDown(KeyCode.P)) return;
			KillStuff();
		}
		
		[Server]
		private void KillStuff() {
			if (!hasBeenTp || hasBeenCleared) return;
			hasBeenCleared = true;
			for (int i = 0; i < _mobs.Count; i = 0)
				_mobs[i].GetAttacked(int.MaxValue);
			Khrom boss = FindObjectOfType<Khrom>();
			if (boss && boss.IsAlive) boss.GetAttacked(int.MaxValue);
		}
	}
}