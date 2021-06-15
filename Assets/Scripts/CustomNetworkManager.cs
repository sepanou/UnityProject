using System;
using System.Collections.Generic;
using DataBanks;
using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity;
using Entity.DynamicEntity.LivingEntity.Mob;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Projectile;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UnityEngine;

public class CustomNetworkManager: NetworkManager {
	public static CustomNetworkManager Instance;
	
	[Header("Scene transition")] 
	[SerializeField] private Animator sceneAnimator;
	
	[Header("Hub Scene")]
	[SerializeField] [Scene] private string hubScene;
	[SerializeField] private Vector3[] hubSpawnPoints;
	
	[Header("Forest Scene")]
	[SerializeField] [Scene] private string forestScene;
	[SerializeField] private Vector3[] forestSpawnPoints;
	
	public readonly List<Player> PlayerPrefabs = new List<Player>();
	public readonly List<Player> AlivePlayers = new List<Player>();
	private Coroutine _sceneTransitionCoroutine;

	public override void Start() {
		base.Start();
		Instance = this;
		
	}

	private void SetPlayerSpawnPoints(IReadOnlyList<Vector3> spawnPoints) {
		for (int i = 0; i < PlayerPrefabs.Count; i++)
			PlayerPrefabs[i].Position = spawnPoints[i % spawnPoints.Count];
	}

	private void CheckAllPlayersAlive(LivingEntity entity) {
		if (!(entity is Player player) || LocalGameManager.Instance.LocalState == LocalGameStates.Hub)
			return;
		AlivePlayers.Remove(player);
		
		if (AlivePlayers.Count != 0) return;
		// Everybody is dead :(
		RemoveSpawnedObjects();
		ServerChangeScene(onlineScene);
		PlayerPrefabs.ForEach(p => p.TargetPrintInfoMessage(p.connectionToClient, "You have not survived Paimpont Forest!"));
	}

	public void PlaySceneTransitionAnimation(string trigger) => sceneAnimator.Play(trigger);

	// Event methods
	public override void OnServerAddPlayer(NetworkConnection conn) {
		GameObject player = Instantiate(playerPrefab, startPositions[startPositionIndex].position, Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn, player);
		Player p = player.GetComponent<Player>();
		// Can't put a switch because Unity won't accept it (source: Maxence), I know it sucks..
		p.playerClass = PlayerPrefabs.Count == 0 ? PlayerClasses.Archer :
			PlayerPrefabs.Count == 1 ? PlayerClasses.Mage : PlayerClasses.Warrior;
		p.OnEntityDie.AddListener(CheckAllPlayersAlive);
		PlayerPrefabs.Add(p);
		AlivePlayers.Add(p);
	}

	private void RemoveSpawnedObjects() {
		List<GameObject> toRemove = new List<GameObject>();
		
		foreach (NetworkIdentity netId in NetworkIdentity.spawned.Values) {
			if (netId.TryGetComponent(out Weapon _) 
			    || netId.TryGetComponent(out Collectibles _) 
			    || netId.TryGetComponent(out Mob _)
			    || netId.TryGetComponent(out Projectile _))
				toRemove.Add(netId.gameObject);
		}
		
		toRemove.ForEach(NetworkServer.Destroy);
	}

	public override void OnServerSceneChanged(string sceneName) {
		if (networkSceneName == forestScene) {
			SetPlayerSpawnPoints(forestSpawnPoints);
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Forest);
		} else if (networkSceneName == hubScene) {
			SetPlayerSpawnPoints(hubSpawnPoints);
			PlayerPrefabs.ForEach(p => p.ResetPlayer());
			PlayerPrefabs.ForEach(p => AlivePlayers.Add(p));
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Hub);
		}
		base.OnServerSceneChanged(sceneName);
	}

	public override void OnClientSceneChanged(NetworkConnection conn) {
		base.OnClientSceneChanged(conn);
		if (IsSceneActive(forestScene))
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Forest);
		else if (IsSceneActive(hubScene))
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Hub);
	}

	public override void OnServerDisconnect(NetworkConnection conn) {
		PlayerPrefabs.Clear();
		base.OnServerDisconnect(conn);
	}

	public override void OnStopClient() {
		base.OnStopClient();
		LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Start);
	}
}
