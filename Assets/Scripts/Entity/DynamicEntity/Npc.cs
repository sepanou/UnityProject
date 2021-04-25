using System;
using System.Collections;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity {
	
	[RequireComponent(typeof(Collider2D))]
	public class Npc: DynamicEntity {
		private enum NpcType { Smith, Seller, Buyer, ClassSelector, StoryTeller }
		
		// A NPC is interactive !
		private Rigidbody2D _rigidBody; // For movements
		private Collider2D _collider;
		// Contains all the players in the interaction area
		// (Player, bool -> has he currently interacting)
		private Dictionary<Player, bool> _playerPool; // Server only
		private bool _canInteract, _isInteracting; // Client only

		[SerializeField] private PlayerClasses classType;
		[SerializeField] private NpcType npcType;

		private void Start() {
			TryGetComponent(out _rigidBody);
			TryGetComponent(out _collider);
			_collider.isTrigger = true;
			_playerPool = new Dictionary<Player, bool>();
			_canInteract = false;
		}

		[ClientCallback]
		private IEnumerator CheckInteraction(Player player) {
			while (_canInteract) {
				while (!InputManager.GetKeyDown("Interact")) {
					if (!_canInteract) {
						yield break;
					}

					yield return null;
				}

				if (!_isInteracting)
					CmdInteract(player);
				
				yield return null;
			}
		}

		[ClientCallback]
		private void StopInteracting() => _isInteracting = false;
		
		private void OnTriggerEnter2D(Collider2D other) {
			if (!other.gameObject.TryGetComponent(out Player player))
				return;
			
			if (isServer)
				_playerPool[player] = false;

			if (!player.isLocalPlayer) return;
			
			_canInteract = true;
			LocalGameManager.Instance.playerInfoManager._displayKey.StartDisplay();
			StopAllCoroutines();
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

		[ServerCallback]
		private bool CanPlayerInteractWithNpc(Player player) => _playerPool.ContainsKey(player);

		[Command(requiresAuthority = false)]
		private void CmdInteract(Player player) {
			if (!CanPlayerInteractWithNpc(player))
				return;

			switch (npcType) {
				case NpcType.Smith:
					InteractSmith(player.connectionToClient, player);
					break;
				case NpcType.Seller:
					InteractSeller(player.connectionToClient, player);
					break;
				case NpcType.Buyer:
					InteractBuyer(player.connectionToClient, player);
					break;
				case NpcType.ClassSelector:
					InteractClassSelector(player.connectionToClient, player);
					break;
				case NpcType.StoryTeller:
					InteractStoryTeller(player.connectionToClient, player);
					break;
			}
		}

		[TargetRpc]
		private void InteractSmith(NetworkConnection target, Player player) {
			InventoryManager.smithInventory.Open();
			// TODO
		}
		
		[TargetRpc]
		private void InteractBuyer(NetworkConnection target, Player player) {
			// TODO
		}
		
		[TargetRpc]
		private void InteractSeller(NetworkConnection target, Player player) {
			// TODO
		}
		
		[TargetRpc]
		private void InteractStoryTeller(NetworkConnection target, Player player) {
			// TODO
		}
		
		[TargetRpc]
		private void InteractClassSelector(NetworkConnection target, Player player) {
			_isInteracting = true;
			switch (classType) {
				case PlayerClasses.Archer:
					PlayerInfoManager.PrintDialog(new [] {"#archer-selector"}, StopInteracting);
					player.CmdSwitchPlayerClass(PlayerClasses.Archer);
					break;
				case PlayerClasses.Mage:
					PlayerInfoManager.PrintDialog(new [] {"#mage-selector"}, StopInteracting);
					player.CmdSwitchPlayerClass(PlayerClasses.Mage);
					break;
				case PlayerClasses.Warrior:
					PlayerInfoManager.PrintDialog(new [] {"#warrior-selector"}, StopInteracting);
					player.CmdSwitchPlayerClass(PlayerClasses.Warrior);
					break;
				default:
					throw new ArgumentException("InteractClassSelector");
			}
		}
	}
}