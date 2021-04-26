using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Scene transition")] 
    [SerializeField] private Animator sceneAnimator;
    
    [Header("Forest Scene Spawn Points")]
    [SerializeField] private string forestSceneName;
    [SerializeField] private Vector3[] forestSpawnPoints;

    private List<Player> _playerPrefabs;

    public override void Start() {
        _playerPrefabs = new List<Player>();
        base.Start();
    }

    private void SetPlayerSpawnPoints(Vector3[] spawnPoints) {
        for (int i = 0; i < _playerPrefabs.Count; i++)
            _playerPrefabs[i].Position = spawnPoints[i % spawnPoints.Length];
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        _playerPrefabs.Add(conn.identity.gameObject.GetComponent<Player>());
    }

    public override void OnServerChangeScene(string newSceneName) {
        if (newSceneName != forestSceneName) return;
        if (sceneAnimator)
            sceneAnimator.Play("StartTransition");
        base.OnServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        if (sceneAnimator)
            sceneAnimator.Play("EndTransition");
        if (networkSceneName != forestSceneName) return;
        AudioDB.PlayMusic("ForestMusic");
        SetPlayerSpawnPoints(forestSpawnPoints);
            
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
        if (sceneAnimator)
            sceneAnimator.Play("StartTransition");
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }
    
    public override void OnClientSceneChanged(NetworkConnection conn) {
        base.OnClientSceneChanged(conn);
        if (sceneAnimator)
            sceneAnimator.Play("EndTransition");
        if (networkSceneName == forestSceneName)
            AudioDB.PlayMusic("ForestMusic");
    }
}
