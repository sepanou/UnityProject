using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

public class CustomNetworkManager: NetworkManager {
	public static CustomNetworkManager Instance;
	
	[Header("Scene transition")] 
	[SerializeField] private Animator sceneAnimator;
	
	[Header("Forest Scene Spawn Points")]
	[SerializeField] private string forestSceneName;
	[SerializeField] private Vector3[] forestSpawnPoints;

	private List<Player> _playerPrefabs;

	public override void Start() {
		_playerPrefabs = new List<Player>();
		base.Start();
		Instance = this;
	}

	public void StartSceneTransition() => sceneAnimator.Play("StartTransition");

	private void SetPlayerSpawnPoints(Vector3[] spawnPoints) {
		for (int i = 0; i < _playerPrefabs.Count; i++)
			_playerPrefabs[i].Position = spawnPoints[i % spawnPoints.Length];
	}
	
	public override void OnServerAddPlayer(NetworkConnection conn) {
		base.OnServerAddPlayer(conn);
		_playerPrefabs.Add(conn.identity.gameObject.GetComponent<Player>());
	}

	public override void OnServerSceneChanged(string sceneName) {
		base.OnServerSceneChanged(sceneName);
		if (sceneAnimator)
			sceneAnimator.Play("EndTransition");
		if (networkSceneName != forestSceneName) return;
		AudioDB.PlayMusic("ForestMusic");
		SetPlayerSpawnPoints(forestSpawnPoints);
			
	}

	public override void OnClientSceneChanged(NetworkConnection conn) {
		base.OnClientSceneChanged(conn);
		if (sceneAnimator)
			sceneAnimator.Play("EndTransition");
		if (networkSceneName == forestSceneName)
			AudioDB.PlayMusic("ForestMusic");
	}

	public override void OnStopClient() {
		LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Start);
		base.OnStopClient();
	}
}
