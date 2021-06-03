using System.Collections;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

public class CustomNetworkManager: NetworkManager {
	public static CustomNetworkManager Instance;
	
	[Header("Scene transition")] 
	[SerializeField] private Animator sceneAnimator;
	
	[Header("Forest Scene")]
	[SerializeField] private string forestSceneName;
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
		PlayerPrefabs.Add(player.GetComponent<Player>());
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
		base.OnStopClient();
		LocalGameManager.Instance.SetLocalGameState(LocalGameStates.Start);
	}
}
