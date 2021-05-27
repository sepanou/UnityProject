using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public class SceneTransfer: NetworkBehaviour { 
	[Header("Destination Scene")]
	[SerializeField] private string sceneToGo;

	private Coroutine _sceneTransitionCoroutine;
	private CustomNetworkManager _networkManager;

	private void Start() => _networkManager = CustomNetworkManager.Instance;

	[ClientCallback] private void OnTriggerEnter2D(Collider2D other) {
		if (!other.TryGetComponent(out Player player) || !player.isLocalPlayer) return;
		CmdStartSceneTransition();
	}

	[ClientRpc] private void RpcStartSceneTransition() 
		=> _networkManager.PlaySceneTransitionAnimation("StartTransition");

	[Server] private IEnumerator LoadingSceneTransition(float delay = 1f) {
		RpcStartSceneTransition();
		yield return new WaitForSeconds(delay);
		_networkManager.ServerChangeScene(sceneToGo);
		_sceneTransitionCoroutine = null;
	}
	
	[Command(requiresAuthority = false)] private void CmdStartSceneTransition() {
		if (_sceneTransitionCoroutine != null) return;
		_sceneTransitionCoroutine = StartCoroutine(LoadingSceneTransition());
	}
}