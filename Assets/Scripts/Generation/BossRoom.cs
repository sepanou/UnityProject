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
	public class BossRoom: NetworkBehaviour {
		[SerializeField] private MobsPrefabsDB mobsAvailable;
		[SerializeField] private GameObject[] mobsSpawnGO;
		[SerializeField] private GameObject bossSpawnPoint;
		[SerializeField] private GameObject bossPrefab;
		private bool hasBeenTp = false;
		private bool hasBeenCleared = false;
		private bool hasBeenWon = false;
		private int mobsToSpawn = 20;
		private Vector3[] _mobSpawns;
		private readonly List<Mob> _mobs = new List<Mob>();
		private InputManager _inputManager;

		[Server]
		private Mob InstantiateRandomMob() {
			Vector3 pos = _mobSpawns[Random.Range(0, _mobSpawns.Length)];
			GameObject mobGO = mobsAvailable.mobsPrefabs[Random.Range(0, mobsAvailable.mobsPrefabs.Length)];
			Instantiate(mobGO, pos, quaternion.identity).TryGetComponent(out Mob mob);
			--mobsToSpawn;
			return mob;
		}
		
		[ClientRpc] private void RpcPlayMusic() => AudioDB.PlayMusic("bossMusic");

		[Server] public void GenerateStuffAndTp() {
			if (hasBeenTp) return;
			hasBeenTp = true;
			_mobSpawns = new Vector3[mobsSpawnGO.Length];
			RpcPlayMusic();
			for (int i = 0; i < _mobSpawns.Length; i++)
				_mobSpawns[i] = mobsSpawnGO[i].transform.position;
			Instantiate(bossPrefab, bossSpawnPoint.transform.position, quaternion.identity).TryGetComponent(out Mob boss);
			boss.OnEntityDie.AddListener(BossDied);
			NetworkServer.Spawn(boss.gameObject);
			while (mobsToSpawn > 0) {
				Mob mob = InstantiateRandomMob();
				mob.OnEntityDie.AddListener(CheckRoomCleared);
				_mobs.Add(mob);
				NetworkServer.Spawn(mob.gameObject);
			}
			CustomNetworkManager.Instance.AlivePlayers.ForEach(playerToTp => {
				playerToTp.transform.position = new Vector3(11, -7, 0);
			});
		}

		[Server] private void CheckRoomCleared(LivingEntity entity) {
			if (!hasBeenTp || hasBeenCleared) return;
			_mobs.Remove(entity as Mob);
			Khrom boss = FindObjectOfType<Khrom>();
			hasBeenCleared = _mobs.Count <= 0 && (!boss || !boss.IsAlive);
		}
		
		[Server] private void KillStuff() {
			if (!hasBeenTp || hasBeenCleared) return;
			for (int i = 0; i < _mobs.Count; i = 0)
				_mobs[i].GetAttacked(int.MaxValue);
			Khrom boss = FindObjectOfType<Khrom>();
			if (boss && boss.IsAlive) boss.GetAttacked(int.MaxValue);
			hasBeenCleared = true;
		}

		[Server] private void BossDied(LivingEntity entity) {
			if (hasBeenCleared) return;
			KillStuff();
		}

		[Command(requiresAuthority = false)]
		private void CmdCheatCode(Player player, string code) {
			if (player.playerName != Player.CheatCodePseudo) return;
			if (code == "Clear" && hasBeenTp && !hasBeenCleared)
				KillStuff();
		}

		private void Update() {
			LocalGameManager manager = LocalGameManager.Instance;
			if (manager.LocalPlayer && manager.LocalPlayer.playerName == Player.CheatCodePseudo && Input.GetKeyDown(KeyCode.P))
				CmdCheatCode(manager.LocalPlayer, "Clear");
			
			if (!isServer || !hasBeenCleared || hasBeenWon) return;
			
			hasBeenWon = true;
			CustomNetworkManager.Instance.WonTheGame();
		}
	}
}