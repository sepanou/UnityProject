using System;
using System.Collections;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity {
	
	[RequireComponent(typeof(Collider2D))]
	public class Npc: DynamicEntity, IInteractiveEntity {
		private enum NpcType { Smith, Seller, Buyer, ClassSelector, StoryTeller }
		
		// A NPC is interactive !
		private Rigidbody2D _rigidBody; // For movements
		private Collider2D _collider;

		[SerializeField] private PlayerClasses classType;
		[SerializeField] private NpcType npcType;

		private void Start() {
			TryGetComponent(out _rigidBody);
			Instantiate();
		}

		[Server]
		public void Interact(Player player) {
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
			switch (classType) {
				case PlayerClasses.Archer:
					PlayerInfoManager.PrintDialog(new [] {"#archer-selector"}, () => StopInteracting(player));
					player.CmdSwitchPlayerClass(PlayerClasses.Archer);
					break;
				case PlayerClasses.Mage:
					PlayerInfoManager.PrintDialog(new [] {"#mage-selector"}, () => StopInteracting(player));
					player.CmdSwitchPlayerClass(PlayerClasses.Mage);
					break;
				case PlayerClasses.Warrior:
					PlayerInfoManager.PrintDialog(new [] {"#warrior-selector"}, () => StopInteracting(player));
					player.CmdSwitchPlayerClass(PlayerClasses.Warrior);
					break;
				default:
					throw new ArgumentException("InteractClassSelector");
			}
		}
	}
}