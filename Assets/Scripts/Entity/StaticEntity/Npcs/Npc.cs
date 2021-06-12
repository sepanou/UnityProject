using System;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Entity.StaticEntity.Npcs {
	
	// For interaction with the player
	[RequireComponent(typeof(Collider2D))]
	public abstract class Npc: Entity, IInteractiveEntity {
		[SerializeField] private Sprite north, south, east, west;
		
		protected abstract void ClientInteract(Player player);

		[TargetRpc] private void TargetInteract(NetworkConnection target, Player player) {
			Vector2 direction = player.Position - (Vector2) transform.position;
			// Circle divided in 4 parts -> angle measurement based on Vector2.up
			direction.Normalize();
			int signedAngle = (int) Vector2.SignedAngle(Vector2.up, direction);
			int index = (int) Math.Round((signedAngle + 360) / 90f, 0, MidpointRounding.AwayFromZero) % 4;
			
			if (index == 0) spriteRenderer.sprite = north;
			else if (index == 1) spriteRenderer.sprite = west;
			else if (index == 2) spriteRenderer.sprite = south;
			else spriteRenderer.sprite = east;
			
			ClientInteract(player);
		}

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