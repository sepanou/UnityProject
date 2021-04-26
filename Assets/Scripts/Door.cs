using System;
using System.Collections;
using System.Collections.Generic;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public class Door: NetworkBehaviour {
	private Collider2D[] _doorCollider;
	private SpriteRenderer _spriteRenderer;

	[SerializeField] private bool isOpen;
	[SerializeField] private Sprite closed;
	[SerializeField] private Sprite opened;
	private HashSet<Player> _playerPool;
	private bool _canInteract;
	[NonSerialized] public static InputManager InputManager;


	private void Start() {
		_doorCollider = GetComponents<Collider2D>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_playerPool = new HashSet<Player>();
		_canInteract = false;
	}


	private void OnTriggerEnter2D(Collider2D other) {
		if (!other.gameObject.TryGetComponent(out Player player))
			return;
			
		if (isServer)
			_playerPool.Add(player);

		if (!player.isLocalPlayer) return;
			
		_canInteract = true;
		LocalGameManager.Instance.playerInfoManager._displayKey.StartDisplay();
		StartCoroutine(CheckInteraction(player));
	}
		
	private void OnTriggerExit2D(Collider2D other) {
		if (!other.gameObject.TryGetComponent(out Player player))
			return;
			
		if (isServer)
			_playerPool.Remove(player);

		if (!player.isLocalPlayer) return;
		
		_canInteract = false;
		LocalGameManager.Instance.playerInfoManager._displayKey.StopDisplay();
	}
	
	[ClientCallback]
	private IEnumerator CheckInteraction(Player player) {
		while (_canInteract) {
			if (InputManager.GetKeyDown("Interact")) {
				CmdToggleDoor(player);
				yield return null;
			}

			yield return null;
		}
	}
	
	[Command(requiresAuthority = false)] 
	private void CmdToggleDoor(Player player) {
		if (!_playerPool.Contains(player)) return;
		_doorCollider[0].enabled = isOpen;
		RpcToggleSprite(isOpen);
		isOpen = !isOpen;
	}

	[ClientRpc]
	private void RpcToggleSprite(bool isOpen2) {
		LocalGameManager.Instance.audioManager.PlayUISound("WoodenDoor");
		_spriteRenderer.sprite = isOpen2 ? closed : opened;
	}
}
