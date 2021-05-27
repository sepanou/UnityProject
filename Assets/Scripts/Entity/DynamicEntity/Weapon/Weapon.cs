using System.Diagnostics.CodeAnalysis;
using Entity.DynamicEntity.LivingEntity.Player;
using DataBanks;
using Mirror;
using UI_Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entity.DynamicEntity.Weapon {
	public abstract class Weapon: DynamicEntity, IInventoryItem, IInteractiveEntity {
		[SyncVar] public Player holder;
		[SyncVar] public PlayerClasses compatibleClass;
		[SyncVar] public bool isGrounded;
		[SyncVar] protected float LastAttackTime; // For cooldown purposes
		[SyncVar] protected bool PlayerFound; // Has the player collected the item?
		
		[SyncVar(hook = nameof(SyncEquippedChanged))] public bool equipped;
		private void SyncEquippedChanged(bool o, bool n) => ChangeWeaponEquipped(n);

		[SerializeField] protected int defaultDamage;
		[SerializeField] protected int specialAttackCost;
		[SerializeField] protected WeaponGeneratorDB weaponGenerator;
		[SerializeField] protected Vector3 defaultCoordsWhenLikedToPlayer;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WritePlayer(holder);
			writer.WriteByte((byte) compatibleClass);
			writer.WriteBoolean(equipped);
			writer.WriteBoolean(isGrounded);
			writer.WriteSingle(LastAttackTime);
			writer.WriteBoolean(PlayerFound);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			holder = reader.ReadPlayer();
			compatibleClass = (PlayerClasses) reader.ReadByte();
			bool newEquipped = reader.ReadBoolean();
			isGrounded = reader.ReadBoolean();
			LastAttackTime = reader.ReadSingle();
			PlayerFound = reader.ReadBoolean();
			
			if (newEquipped == equipped) return;
			SyncEquippedChanged(equipped, newEquipped);
			equipped = newEquipped;
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		protected new void Instantiate() {
			base.Instantiate();
			
			if (isServer) {
				isGrounded = true; // By default, weapon on the ground !
				PlayerFound = false;
				LastAttackTime = -1f;
			}

			if (holder) SetActive(equipped);

			SceneManager.sceneLoaded += (scene, module) => SetActive(equipped);
			AutoStopInteracting = true;
			InteractionCondition = player => !PlayerFound 
			                                 && isGrounded
			                                 && player.playerClass == compatibleClass
			                                 && !player.IsFullInventory();
		}

		private void SetActive(bool state) {
			spriteRenderer.color = new Color(255, 255, 255, state ? 255 : 0);
			enabled = state;
		}
		
		public int GetDamage() => defaultDamage;
		public int GetSpecialAttackCost() => specialAttackCost;

		public bool CanAttack() => holder && equipped && (LastAttackTime < 0 || !(Time.time - LastAttackTime < Speed));

		protected abstract void DefaultAttack();
		protected abstract void SpecialAttack();
		public abstract RectTransform GetInformationPopup();
		public abstract string GetName();

		[Server] public void Interact(Player player) {
			PlayerFound = true;
			StartCoroutine(Collectibles.Collectibles.OnTargetDetected(this, player));
		}

		// Validation checks before attacking
		// Only the player with authority on the object can call this method
		[Server] public void UseWeapon(bool fireOneButton, bool fireTwoButton) {
			if (fireOneButton) DefaultAttack();
			else if (fireTwoButton && holder.HasEnoughEnergy(specialAttackCost)) SpecialAttack();
		}

		[Server] public void Equip(Player source) {
			holder = source;
			equipped = true;
		}

		[Server] public void UnEquip() => equipped = false;

		[Client] private void ChangeWeaponEquipped(bool state) {
			SetActive(state);
			if (!state) return;
			transform.localPosition = defaultCoordsWhenLikedToPlayer;
		}

		[ClientRpc] public void RpcSetWeaponParent(Transform parent) => transform.parent = parent;

		[TargetRpc] public void TargetSetClientAuthority(NetworkConnection target, bool state) {
			if (TryGetComponent(out NetworkTransform netTransform))
				netTransform.clientAuthority = state;
		}

		// Called on client every time this object is spawned (especially when new players join)
		[ClientCallback] public override void OnStartClient() {
			if (!holder) return;
			Transform tmpTransform = transform;
			tmpTransform.parent = holder.transform;
			tmpTransform.position = defaultCoordsWhenLikedToPlayer;
		}
	}
	
	public static class WeaponSerialization {
		public static void WriteWeapon(this NetworkWriter writer, Weapon weapon) {
			writer.WriteBoolean(weapon);
			if (weapon && weapon.netIdentity)
				writer.WriteNetworkIdentity(weapon.netIdentity);
		}

		public static Weapon ReadWeapon(this NetworkReader reader) {
			if (!reader.ReadBoolean()) return null;
			NetworkIdentity identity = reader.ReadNetworkIdentity();
			return !identity ? null : identity.GetComponent<Weapon>();
		}
	}
}