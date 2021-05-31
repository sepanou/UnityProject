using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UI_Audio.Inventories;
using UnityEngine;

namespace Entity {
	public interface IInteractiveEntity {
		[Server] void Interact(Player player);
	}
	
	public abstract class Entity: NetworkBehaviour {
		[SerializeField] protected SpriteRenderer spriteRenderer;
		[SerializeField] private Collider2D interactionCollider;

		protected bool AutoStopInteracting;
		protected Func<Player, bool> InteractionCondition;
		// <Player, bool : is he currently interacting with the object>
		private Dictionary<Player, bool> _playerPool;
		private bool _canInteract;
		private IEnumerator _checkInteractionCoroutine;
		private IInteractiveEntity _interactive;

		[NonSerialized] protected static LocalGameManager Manager;
		[NonSerialized] protected static InputManager InputManager;
		[NonSerialized] protected static PlayerInfoManager PlayerInfoManager;
		[NonSerialized] protected static InventoryManager InventoryManager;

		public static void InitClass(LocalGameManager manager) {
			if (Manager) throw new Exception("InitClass called multiple times");
			Manager = manager;
			PlayerInfoManager = Manager.playerInfoManager;
			InventoryManager = Manager.inventoryManager;
			InputManager = Manager.inputManager;
		}

		public static void SetRenderingLayersInChildren(int sortingLayerID, string sortingLayerName, int layerMask, GameObject gameObject) {
			if (!gameObject.activeInHierarchy) return;
			gameObject.layer = layerMask;
			if (gameObject.TryGetComponent(out SpriteRenderer renderer)) {
				renderer.sortingLayerName = sortingLayerName;
				renderer.sortingLayerID = sortingLayerID;
			}
			if (gameObject.TryGetComponent(out ParticleSystemRenderer psRenderer)) {
				psRenderer.sortingLayerName = sortingLayerName;
				psRenderer.sortingLayerID = sortingLayerID;
			}
			for (int i = 0; i < gameObject.transform.childCount; ++i) {
				GameObject child = gameObject.transform.GetChild(i).gameObject;
				SetRenderingLayersInChildren(sortingLayerID, sortingLayerName, layerMask, child);
			}
		}
		
		protected static void SetSameRenderingParameters(Entity reference, Entity toChange) {
			// Two GO with the same layer are assumed to share the same renderer parameters
			if (toChange.gameObject.layer == reference.gameObject.layer)
				return;
			if (!reference.spriteRenderer) {
				SetRenderingLayersInChildren(reference.spriteRenderer.sortingLayerID,
					reference.spriteRenderer.sortingLayerName, reference.gameObject.layer, toChange.gameObject);
				return;
			}
			if (!reference.TryGetComponent(out ParticleSystemRenderer psRenderer)) return;
			SetRenderingLayersInChildren(psRenderer.sortingLayerID, psRenderer.sortingLayerName, 
				reference.gameObject.layer, toChange.gameObject);
		}
		
		protected void Instantiate() {
			if (!spriteRenderer)
				spriteRenderer = GetComponent<SpriteRenderer>();
			InteractionCondition = null;
			AutoStopInteracting = false;
			_canInteract = false;
			_playerPool = new Dictionary<Player, bool>();
			_interactive = interactionCollider && this is IInteractiveEntity interactive ? interactive : null;
		}

		public override void OnStartClient() {
			base.OnStartClient();
			CmdApplyLayers();
		}

		public SpriteRenderer GetSpriteRenderer() => spriteRenderer;

		public Vector2 Position {
			get => transform.position;
			
			[Server]
			set {
				Transform tempTransform = transform;
				tempTransform.position = new Vector3(value.x, value.y, tempTransform.position.z);
			}
		}

		[Command(requiresAuthority = false)]
		private void CmdApplyLayers(NetworkConnectionToClient target = null) {
			if (spriteRenderer)
				TargetSetSortingLayer(target, spriteRenderer.sortingLayerID, gameObject.layer, spriteRenderer.enabled);
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc]
		private void TargetSetSortingLayer(NetworkConnection target, int sortingLayerId, int layerMaskId, bool rendererEnabled) {
			if (spriteRenderer) {
				spriteRenderer.sortingLayerID = sortingLayerId;
				spriteRenderer.enabled = rendererEnabled;
			}
			gameObject.layer = layerMaskId;
		}
		
		// *-*-*-*-*- For interactive objects (NPC / Doors / ...) -*-*-*-*-*
		
		[Client] private IEnumerator CheckInteraction(Player player) {
			while (_canInteract) {
				while (!InputManager.GetKeyDown("Interact")) {
					if (!_canInteract)
						yield break;
					yield return null;
				}

				if (!_playerPool[player] || AutoStopInteracting)
					CmdTryInteract(player);

				yield return null;
			}
		}

		[Client] public void StopInteracting(Player player) {
			// For callbacks from client (player) 
			SetIsInteractive(player, false);
			// Tell it to the server
			CmdStopInteracting(player);
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetSetIsInteractive(NetworkConnection target, Player player, bool state) 
			=> SetIsInteractive(player, state);

		[Command(requiresAuthority = false)] private void CmdTryInteract(Player player) {
			if (InteractionCondition != null && !InteractionCondition(player))
				return;
			if (AutoStopInteracting) {
				_interactive.Interact(player);
				return;
			}
			if (!_playerPool.ContainsKey(player) || _playerPool[player]) return;
			SetIsInteractive(player, true);
			TargetSetIsInteractive(player.connectionToClient, player, true);
			_interactive.Interact(player);
		}

		[Command(requiresAuthority = false)]
		private void CmdStopInteracting(Player player) => SetIsInteractive(player, false);

		private void SetIsInteractive(Player player, bool state) {
			if (!_playerPool.ContainsKey(player)) return;
			_playerPool[player] = state;
		}

		[Server] public bool VerifyInteractionWith(Player player) 
			=> _playerPool.ContainsKey(player) && _playerPool[player];

		public void DisableInteraction(Player player) {
			interactionCollider.enabled = false;
			if (netId == 0) return; // == not networked yet
			if (_checkInteractionCoroutine != null)
				StopCoroutine(_checkInteractionCoroutine);
			_playerPool.Clear();
			if (!player.isLocalPlayer) return;
			_canInteract = false;
			PlayerInfoManager.displayKey.StopDisplay();
		}

		[ClientRpc] public void RpcDisableInteraction(Player player) => DisableInteraction(player);

		[ClientRpc] public void RpcEnableInteraction() => interactionCollider.enabled = true;

		protected virtual void OnTriggerEnter2D(Collider2D other) {
			if (_interactive == null || !other.gameObject.TryGetComponent(out Player player))
				return;
			if (InteractionCondition != null && !InteractionCondition(player))
				return;
			_playerPool[player] = false;
			if (!player.isLocalPlayer) return;
			_canInteract = true;
			PlayerInfoManager.displayKey.StartDisplay();
			if (_checkInteractionCoroutine != null)
				StopCoroutine(_checkInteractionCoroutine);
			_checkInteractionCoroutine = CheckInteraction(player);
			StartCoroutine(_checkInteractionCoroutine);
		}
		
		protected virtual void OnTriggerExit2D(Collider2D other) {
			if (_interactive == null || !other.gameObject.TryGetComponent(out Player player))
				return;
			if (InteractionCondition != null && !InteractionCondition(player))
				return;
			_playerPool.Remove(player);
			if (!player.isLocalPlayer) return;
			_canInteract = false;
			PlayerInfoManager.displayKey.StopDisplay();
		}
	}
}
