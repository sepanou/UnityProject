using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public class SceneTransfer: NetworkBehaviour { 
	[Header("Destination Scene")]
	[SerializeField] [Scene] private string sceneToGo;
	
	private CustomNetworkManager _networkManager;

	private void Start() => _networkManager = CustomNetworkManager.Instance;

	[ClientCallback] private void OnTriggerEnter2D(Collider2D other) {
		if (!other.TryGetComponent(out Player player) || !player.isLocalPlayer) return;
		CmdStartSceneTransition();
	}

	[ClientRpc] private void RpcStartSceneTransition() 
		=> _networkManager.PlaySceneTransitionAnimation("StartTransition");

	[Server]
	private IEnumerator WaitAndChangeScene(float delay) {
		yield return new WaitForSeconds(delay);
		_networkManager.ServerChangeScene(sceneToGo);
	}

	[Command(requiresAuthority = false)] private void CmdStartSceneTransition() {
		RpcStartSceneTransition();
		_networkManager.PlaySceneTransitionAnimation("StartTransition");
		StartCoroutine(WaitAndChangeScene(1f));
	}
}