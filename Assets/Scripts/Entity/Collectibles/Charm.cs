using System;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.Collectibles {
	[Serializable]
	public class CharmData {
		public const float MaxDefaultAttackDamageBonus = 0.25f, 
			MaxSpecialAttackDamageBonus = 0.5f,
			MaxSpeedBonus = 0.2f,
			MaxCooldownReduction = 0.2f;
		public const int MaxHealthBonus = 30, MaxPowerBonus = 50;
		
		public float defaultAttackDamageBonus, specialAttackDamageBonus;
		public int healthBonus, powerBonus;
		public float speedBonus, cooldownReduction;
		public string name;
		
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
				cooldownReduction = first.cooldownReduction + second.cooldownReduction,
				name = second.name
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
				cooldownReduction = first.cooldownReduction - second.cooldownReduction,
				name = first.name
			};
		}

		public static int GetKibryValue(CharmData charmData) {
			float kibryValue = 0f;
			kibryValue += 100f * charmData.defaultAttackDamageBonus / MaxDefaultAttackDamageBonus;
			kibryValue += 100f * charmData.specialAttackDamageBonus / MaxSpecialAttackDamageBonus;
			kibryValue += 100f * charmData.speedBonus / MaxSpeedBonus;
			kibryValue += 100f * charmData.cooldownReduction / MaxCooldownReduction;
			kibryValue += 100f * charmData.healthBonus / MaxHealthBonus;
			kibryValue += 100f * charmData.powerBonus / MaxPowerBonus;
			return (int) kibryValue;
		}
	}
	
	public class Charm: Collectibles, IInventoryItem, IInteractiveEntity {
		[SyncVar] [ShowInInspector] public CharmData bonuses;
		
		// Index of the sprite in the WPGenerator array -> Mirror can't serialize sprites
		[SyncVar(hook = nameof(SyncSpriteIndexChanged))] [NonSerialized] public int SpriteIndex;
		private void SyncSpriteIndexChanged(int o, int n) 
			=> spriteRenderer.sprite = WeaponGenerator.GetCharmSprite(n);
		
		[SyncVar(hook = nameof(SyncIsGroundedChanged))] private bool _isGrounded = true;
		private void SyncIsGroundedChanged(bool o, bool n) => SetSpriteRendererVisible(n);

		public override void OnStartServer() {
			base.OnStartServer();
			Instantiate();
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (!isServer) Instantiate();
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteBoolean(_isGrounded);
			writer.Write(bonuses);
			writer.WriteInt32(SpriteIndex);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			bool newIsGrounded = reader.ReadBoolean(); 
			bonuses = reader.Read<CharmData>();
			int newSpriteIndex = reader.ReadInt32();

			if (newSpriteIndex != SpriteIndex) {
				SyncSpriteIndexChanged(SpriteIndex, newSpriteIndex);
				SpriteIndex = newSpriteIndex;
			}
			
			if (newIsGrounded == _isGrounded) return;
			SyncIsGroundedChanged(_isGrounded, newIsGrounded);
			_isGrounded = newIsGrounded;
		}

		public RectTransform GetInformationPopup() 
			=> PlayerInfoManager.ShowCharmDescription(bonuses);

		public int GetKibryValue() => CharmData.GetKibryValue(bonuses);

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