using Entity.DynamicEntity.LivingEntity.Player;
using System.Collections.Generic;
using DataBanks;
using Mirror;
using UI_Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entity.DynamicEntity.Weapon {
	public abstract class Weapon: DynamicEntity, IInventoryItem {
		private ContactFilter2D _filter;
		
		[SyncVar] public Player holder;
		[SyncVar] public PlayerClasses compatibleClass;
		[SyncVar] public bool equipped;
		[SyncVar] public bool isGrounded;
		[SyncVar] protected float LastAttackTime; // For cooldown purposes
		[SyncVar] protected bool PlayerFound; // Has the player collected the item?

		[SerializeField] protected int defaultDamage;
		[SerializeField] protected int specialAttackCost;
		[SerializeField] protected WeaponGeneratorDB weaponGenerator;
		[SerializeField] protected Vector3 defaultCoordsWhenLikedToPlayer;
		
		public static event ChangedWeapon OnWeaponChange;
		public delegate void ChangedWeapon(Weapon weapon);

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
			equipped = reader.ReadBoolean();
			isGrounded = reader.ReadBoolean();
			LastAttackTime = reader.ReadSingle();
			PlayerFound = reader.ReadBoolean();
		}

		protected new void Instantiate() {
			if (isServer) {
				isGrounded = true; // By default, weapon on the ground !
				PlayerFound = false;
				LastAttackTime = -1f;
			}
			_filter = new ContactFilter2D();

			base.Instantiate();
			
			if (holder) SetActive(equipped);

			SceneManager.sceneLoaded += (scene, module) => SetActive(equipped);
		}

		private void SetActive(bool state) {
			spriteRenderer.color = new Color(255, 255, 255, state ? 255 : 0);
			enabled = state;
		}
		
		public int GetDamage() => defaultDamage;
		public int GetSpecialAttackCost() => specialAttackCost;

		public bool CanAttack() {
			return holder && equipped &&
				   (LastAttackTime < 0 || !(Time.time - LastAttackTime < Speed));
		}
		
		protected abstract void DefaultAttack();
		protected abstract void SpecialAttack();
		public abstract RectTransform GetInformationPopup();
		public abstract string GetName();

		[ServerCallback]
		private bool CheckForCompatibleNearbyPlayers(out Player compatiblePlayer) {
			List<Collider2D> results = new List<Collider2D>();
			Physics2D.OverlapCircle(transform.position, 2f, _filter.NoFilter(), results);
			
			foreach (Collider2D obj in results) {
				if (!obj.gameObject.TryGetComponent(out Player player)
				    || player.playerClass != compatibleClass
            || player.IsFullInventory()) continue;
				compatiblePlayer = player;
				return true;
			}

			compatiblePlayer = null;
			return false;
		}
		
		[ServerCallback]
		protected void GroundedLogic() {
			if (!isGrounded || !CheckForCompatibleNearbyPlayers(out Player target)) return;
			PlayerFound = true;
			StartCoroutine(Collectibles.Collectibles.OnTargetDetected(this, target));
		}
		
		// Validation checks before attacking
		// Only the player with authority on the object can call this method
		[ServerCallback]
		public void UseWeapon(bool fireOneButton, bool fireTwoButton) {
			if (fireOneButton) DefaultAttack();
			else if (fireTwoButton && holder.HasEnoughEnergy(specialAttackCost)) SpecialAttack();
		}

		[ServerCallback]
		public void Equip(Player source) {
			holder = source;
			equipped = true;
			RpcEquip();
		}
		
		[ServerCallback]
		public void UnEquip() {
			equipped = false;
			RpcUnEquip();
		}

		[ClientRpc]
		private void RpcEquip() {
			transform.localPosition = defaultCoordsWhenLikedToPlayer;
			SetActive(true);
			if (holder.isLocalPlayer)
				OnWeaponChange?.Invoke(this);
		}

		[ClientRpc] private void RpcUnEquip() => SetActive(false);

		[ClientRpc] public void RpcSetWeaponParent(Transform parent) => transform.parent = parent;

		// Called on client every time this object is spawned (especially when new players join)
		[ClientCallback]
		public override void OnStartClient() {
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