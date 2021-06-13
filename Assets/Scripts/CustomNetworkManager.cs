using System;
using System.Collections.Generic;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public class CustomNetworkManager: NetworkManager {
	public static CustomNetworkManager Instance;
	
	[Header("Scene transition")] 
	[SerializeField] private Animator sceneAnimator;
	
	[Header("Forest Scene")]
	[SerializeField] [Scene] private string forestScene;
	[SerializeField] private Vector3[] forestSpawnPoints;

	public readonly List<Player> PlayerPrefabs = new List<Player>();
	private Coroutine _sceneTransitionCoroutine;

	public override void Start() {
		base.Start();
		Instance = this;
	}

	private void SetPlayerSpawnPoints(IReadOnlyList<Vector3> spawnPoints) {
		for (int i = 0; i < PlayerPrefabs.Count; i++)
			PlayerPrefabs[i].Position = spawnPoints[i % spawnPoints.Count];
	}

	public void PlaySceneTransitionAnimation(string trigger) => sceneAnimator.Play(trigger);

	// Event methods
	public override void OnServerAddPlayer(NetworkConnection conn) {
		GameObject player = Instantiate(playerPrefab, startPositions[startPositionIndex].position, Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn, player);
		Player p = player.GetComponent<Player>();
		// Can't put a switch because Unity won't accept it (source: Maxence), I know it sucks..
		p.playerClass = PlayerPrefabs.Count == 0 ? PlayerClasses.Archer :
			PlayerPrefabs.Count == 2 ? PlayerClasses.Mage : PlayerClasses.Warrior;
		PlayerPrefabs.Add(p);
	}

	public override void OnServerSceneChanged(string sceneName) {
		base.OnServerSceneChanged(sceneName);
		if (networkSceneName != forestScene) return;
		SetPlayerSpawnPoints(forestSpawnPoints);
		LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Forest);
	}

	public override void OnClientSceneChanged(NetworkConnection conn) {
		base.OnClientSceneChanged(conn);
		if (IsSceneActive(forestScene))
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Forest);
	}

	public override void OnServerDisconnect(NetworkConnection conn) {
		try {
			Player player = PlayerPrefabs.Find(search => search.connectionToClient == conn);
			FileStorage.SavePlayerOrchid(player.playerName, player.Orchid);
			PlayerPrefabs.Remove(player);
		}
		catch (ArgumentNullException){}
		base.OnClientDisconnect(conn);
	}

	public override void OnStopClient() {
		base.OnStopClient();
		LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Start);
	}
}
