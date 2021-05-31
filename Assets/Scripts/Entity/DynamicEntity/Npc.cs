using System;
using System.Diagnostics.CodeAnalysis;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Entity.DynamicEntity {
	
	[RequireComponent(typeof(Collider2D))]
	public class Npc: DynamicEntity, IInteractiveEntity {
		public enum NpcType { Smith, Orchidologist, Collector, InnKeeper, ClassSelector, StoryTeller }
		
		// A NPC is interactive !
		// ReSharper disable once NotAccessedField.Local
		private Rigidbody2D _rigidBody; // For movements
		private Collider2D _collider;

		[SerializeField] private PlayerClasses classType;
		[SerializeField] private NpcType npcType;
		public NpcType GetNpcType => npcType;
		
		private void Start() {
			Instantiate();
			TryGetComponent(out _rigidBody);
		}

		[Server] public void Interact(Player player) {
			switch (npcType) {
				case NpcType.Smith:
					TargetInteractSmith(player.connectionToClient, player);
					break;
				case NpcType.InnKeeper:
					TargetInteractInnKeeper(player.connectionToClient, player);
					break;
				case NpcType.Collector:
					TargetInteractCollector(player.connectionToClient, player);
					break;
				case NpcType.Orchidologist:
					TargetInteractOrchidologist(player.connectionToClient, player);
					break;
				case NpcType.ClassSelector:
					TargetInteractClassSelector(player.connectionToClient, player);
					break;
				case NpcType.StoryTeller:
					TargetInteractStoryTeller(player.connectionToClient, player);
					break;
				default:
					throw new ArgumentException("Unknown NPC Type !");
			}
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractSmith(NetworkConnection target, Player player) {
			InventoryManager.OpenShopKeeperInventory(NpcType.Smith, this);
			// TODO
		}
		
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractCollector(NetworkConnection target, Player player) {
			InventoryManager.OpenShopKeeperInventory(NpcType.Collector, this);
			// TODO
		}
		
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractInnKeeper(NetworkConnection target, Player player) {
			InventoryManager.OpenShopKeeperInventory(NpcType.InnKeeper, this);
			// TODO
		}
		
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractOrchidologist(NetworkConnection target, Player player) {
			InventoryManager.OpenShopKeeperInventory(NpcType.Orchidologist, this);
			// TODO
		}
		
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractStoryTeller(NetworkConnection target, Player player) {
			// TODO
		}
		
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[TargetRpc] private void TargetInteractClassSelector(NetworkConnection target, Player player) {
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

		[Client] public void PrintDialog(string[] dialogKeys, UnityAction callback, bool sudden = false) =>
			PlayerInfoManager.PrintDialog(dialogKeys, callback, sudden);

		[ClientCallback] protected new void OnTriggerExit2D(Collider2D other) {
			base.OnTriggerExit2D(other);
			InventoryManager.CloseAllInventories();
		}
	}
	
	public static class NpcSerialization {
		public static void WriteNpc(this NetworkWriter writer, Npc npc) {
			writer.WriteBoolean(npc);
			if (npc && npc.netIdentity)
				writer.WriteNetworkIdentity(npc.netIdentity);
		}

		public static Npc ReadNpc(this NetworkReader reader) {
			if (!reader.ReadBoolean()) return null;
			NetworkIdentity identity = reader.ReadNetworkIdentity();
			return !identity ? null : identity.GetComponent<Npc>();
		}
	}
}