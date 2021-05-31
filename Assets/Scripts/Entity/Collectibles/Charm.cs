using System;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
using UI_Audio.Inventories;
using UnityEngine;

namespace Entity.Collectibles {
	[Serializable]
	public class CharmData {
		public float defaultAttackDamageBonus, specialAttackDamageBonus;
		public int healthBonus, powerBonus;
		public float speedBonus, cooldownReduction;
		
		public static CharmData operator +(CharmData first, CharmData second) {
			if (first == null)
				return second;
			if (second == null)
				return first;
			return new CharmData {
				defaultAttackDamageBonus = first.defaultAttackDamageBonus + second.defaultAttackDamageBonus,
				specialAttackDamageBonus = first.specialAttackDamageBonus + second.specialAttackDamageBonus,
				healthBonus = first.healthBonus + second.healthBonus,
				powerBonus = first.powerBonus + second.powerBonus,
				speedBonus = first.speedBonus + second.speedBonus,
				cooldownReduction = first.cooldownReduction + second.cooldownReduction
			};
		}

		public static CharmData operator -(CharmData first, CharmData second) {
			if (first == null)
				return second;
			if (second == null)
				return first;
			return new CharmData {
				defaultAttackDamageBonus = first.defaultAttackDamageBonus - second.defaultAttackDamageBonus,
				specialAttackDamageBonus = first.specialAttackDamageBonus - second.specialAttackDamageBonus,
				healthBonus = first.healthBonus - second.healthBonus,
				powerBonus = first.powerBonus - second.powerBonus,
				speedBonus = first.speedBonus - second.speedBonus,
				cooldownReduction = first.cooldownReduction - second.cooldownReduction
			};
		}
	}
	
	public class Charm: Collectibles, IInventoryItem, IInteractiveEntity {
		[SyncVar] [HideInInspector] public CharmData bonuses;
		
		[SyncVar(hook = nameof(SyncIsGroundedChanged))] private bool _isGrounded = true;
		private void SyncIsGroundedChanged(bool o, bool n) => spriteRenderer.enabled = n;

		private void Start() => Instantiate();

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteBoolean(_isGrounded);
			writer.Write(bonuses);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			bool newIsGrounded = reader.ReadBoolean(); 
			bonuses = reader.Read<CharmData>();
			if (newIsGrounded == _isGrounded) return;
			SyncIsGroundedChanged(_isGrounded, newIsGrounded);
			_isGrounded = newIsGrounded;
		}

		public RectTransform GetInformationPopup() 
			=> !PlayerInfoManager ? null : PlayerInfoManager.ShowCharmDescription(bonuses);

		[Server] public void SetIsGrounded(bool state) => _isGrounded = state;

		[Server] public void Interact(Player player)
			=> StartCoroutine(OnTargetDetected(this, player));
	}
	
	public static class CharmSerialization {
		public static void WriteCharm(this NetworkWriter writer, Charm charm) {
			writer.WriteBoolean(charm);
			if (charm && charm.netIdentity)
				writer.WriteNetworkIdentity(charm.netIdentity);
		}

		public static Charm ReadCharm(this NetworkReader reader) {
			if (!reader.ReadBoolean()) return null;
			NetworkIdentity identity = reader.ReadNetworkIdentity();
			return !identity ? null : identity.GetComponent<Charm>();
		}
	}
}