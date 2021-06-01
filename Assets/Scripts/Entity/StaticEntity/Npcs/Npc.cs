using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Entity.StaticEntity.Npcs {
	
	// For interaction with the player
	[RequireComponent(typeof(Collider2D))]
	public abstract class Npc: Entity, IInteractiveEntity {
		protected abstract void TargetInteract(NetworkConnection target, Player player);

		[Server] public void Interact(Player player) => TargetInteract(player.connectionToClient, player);
		
		[Client] public void PrintDialog(string[] dialogKeys, UnityAction callback, bool sudden = false) =>
			PlayerInfoManager.PrintDialog(dialogKeys, callback, sudden);
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