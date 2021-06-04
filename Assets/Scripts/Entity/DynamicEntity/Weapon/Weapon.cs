using System;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entity.DynamicEntity.Weapon {
	public abstract class Weapon: DynamicEntity, IInventoryItem, IInteractiveEntity {
		[SyncVar] protected Player Holder;
		[SyncVar] public PlayerClasses compatibleClass;
		[SyncVar] private bool _playerFound; // Has the player collected the item?
		
		// Index of the sprite in the WPGenerator array -> Mirror can't serialize sprites
		[SyncVar(hook = nameof(SyncSpriteIndexChanged))] [NonSerialized] public int SpriteIndex; 
		private void SyncSpriteIndexChanged(int o, int n) 
			=> spriteRenderer.sprite = WeaponGenerator.GetWeaponSprite(this, n);
		
		[SyncVar(hook = nameof(SyncIsGroundedChanged))] protected bool IsGrounded;
		private void SyncIsGroundedChanged(bool o, bool n) => SetSpriteRendererVisible(n);
		
		[SerializeField] public Vector3 defaultCoordsWhenLikedToPlayer;
		[SerializeField] protected int defaultDamage, specialDamage;
		[SerializeField] protected int specialAttackCost;
		private float _lastAttackTime; // For cooldown purposes
		// Makes sure that the cooldown of the last attack is not modified if the player removes / add Charms
		private float _lastAttackCooldown;
		private bool _isSpecial; // True if the last attack was a special one
		
		protected bool Equipped => Holder && Holder.HasWeaponEquipped(this);

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WritePlayer(Holder);
			writer.WriteByte((byte) compatibleClass);
			writer.WriteBoolean(IsGrounded);
			writer.WriteSingle(_lastAttackTime);
			writer.WriteBoolean(_playerFound);
			writer.WriteInt32(SpriteIndex);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			Holder = reader.ReadPlayer();
			compatibleClass = (PlayerClasses) reader.ReadByte();
			bool newIsGrounded = reader.ReadBoolean();
			_lastAttackTime = reader.ReadSingle();
			_playerFound = reader.ReadBoolean();
			int newSpriteIndex = reader.ReadInt32();

			if (IsGrounded != newIsGrounded) {
				SyncIsGroundedChanged(IsGrounded, newIsGrounded);
				IsGrounded = newIsGrounded;
			}

			if (newSpriteIndex == SpriteIndex) return;
			SyncSpriteIndexChanged(SpriteIndex, newSpriteIndex);
			SpriteIndex = newSpriteIndex;
		}

		// Called after OnStartClient(), OnStartServer(), ...
		// Do not move this instruction to Instantiate - invisible weapons when joining otherwise
		private void Start() => SetSpriteRendererVisible(Equipped || IsGrounded);

		protected new void Instantiate() {
			base.Instantiate();
			
			if (isServer) {
				IsGrounded = true; // By default, weapon on the ground !
				_playerFound = false;
				_lastAttackTime = -1f;
			}

			if (Holder) SetSpriteRendererVisible(Equipped);

			SceneManager.sceneLoaded += (scene, module) => {
				if (this) SetSpriteRendererVisible(Equipped); // Don't change
			};
			
			AutoStopInteracting = true;
			InteractionCondition = player => !_playerFound 
			                                 && IsGrounded
			                                 && player.playerClass == compatibleClass
			                                 && !player.IsFullInventory();
		}

		protected int GetDamage() => (int) ((Holder
			? Holder.ApplyDamageBonuses(_isSpecial ? specialDamage : defaultDamage, _isSpecial)
			: _isSpecial ? specialDamage : defaultDamage) * GetDamageMultiplier(_isSpecial));
		
		// Used for projectile - they have a bool telling whether they were launched on a special or default attack
		public int GetDamage(bool isSpecial) => (int) ((Holder
			? Holder.ApplyDamageBonuses(isSpecial ? specialDamage : defaultDamage, isSpecial)
			: _isSpecial ? specialDamage : defaultDamage) * GetDamageMultiplier(isSpecial));

		public int GetSpecialAttackCost() => specialAttackCost;

		public bool CanAttack()
			=> Holder && Equipped && (_lastAttackTime < 0 || !(Time.time - _lastAttackTime < _lastAttackCooldown));

		protected abstract void DefaultAttack();
		protected abstract void SpecialAttack();
		protected abstract float GetDamageMultiplier(bool isSpecial);
		public abstract RectTransform GetInformationPopup();
		public abstract int GetKibryValue();
		public abstract string GetWeaponName();

		[Server] public void SetIsGrounded(bool state) => IsGrounded = state;
		
		[Server] public void Interact(Player player) {
			_playerFound = true;
			StartCoroutine(Collectibles.Collectibles.OnTargetDetected(this, player));
		}

		// Validation checks before attacking
		// Only the player with authority on the object can call this method
		[Server] public void UseWeapon(bool fireOneButton, bool fireTwoButton) {
			_lastAttackTime = Time.time;
			_lastAttackCooldown = Holder ? Holder.ApplyCooldownBonuses(Speed) : Speed;
			
			if (fireOneButton) {
				_isSpecial = false;
				DefaultAttack();
				TargetLaunchAttackCooldown(connectionToClient, _lastAttackCooldown, _lastAttackTime, false);
			}
			else if (fireTwoButton && Holder.HasEnoughEnergy(specialAttackCost)) {
				Holder.ReduceEnergy(specialAttackCost);
				_isSpecial = true;
				SpecialAttack();
				TargetLaunchAttackCooldown(connectionToClient, _lastAttackCooldown, _lastAttackTime, true);
			}
		}

		[Server] public void LinkToPlayer(Player player) {
			// Interactions + Authority
			netIdentity.AssignClientAuthority(player.netIdentity.connectionToClient);
			IsGrounded = false;
			DisableInteraction(player);
			RpcDisableInteraction(player);
			// Transform
			transform.SetParent(player.transform, false);
			// Set owner
			Holder = player;
			// Target authority for synchronization of networkTransforms
			if (!TryGetComponent(out NetworkTransform netTransform)) return; // Should never happen
			netTransform.clientAuthority = true;
			TargetSetClientAuthority(connectionToClient, true);
		}

		[Server] public void Drop(Player player) {
			if (!Holder) return;
			// Interactions
			IsGrounded = true;
			_playerFound = false;
			EnableInteraction();
			RpcEnableInteraction();
			// Target authority for synchronization of networkTransforms
			if (TryGetComponent(out NetworkTransform netTransform))
				netTransform.clientAuthority = false;
			TargetSetClientAuthority(connectionToClient, false);
			netIdentity.RemoveClientAuthority();
			// Transform
			Transform current = transform;
			current.SetParent(null, false);
			current.localPosition = Vector3.zero;
			current.position = player.transform.position;
			// Set owner
			Holder.RemoveWeapon(this);
			Holder = null;
		}

		[TargetRpc] private void TargetSetClientAuthority(NetworkConnection target, bool state) {
			if (TryGetComponent(out NetworkTransform netTransform))
				netTransform.clientAuthority = state;
		}

		[TargetRpc]
		private void TargetLaunchAttackCooldown(NetworkConnection target, float duration, float startTime, bool isSpecial) {
			_lastAttackTime = startTime;
			_lastAttackCooldown = duration;
			float timeLeft = duration + startTime - Time.time;
			
			if (!isSpecial)
				Manager.playerInfoManager.StartDefaultAttackCooldown(timeLeft);
			else
				Manager.playerInfoManager.StartSpecialAttackCooldown(timeLeft);
		}

		// Called on client every time this object is spawned (especially when new players join)
		[ClientCallback] public override void OnStartClient() {
			if (!Holder) return;
			Transform tmpTransform = transform;
			tmpTransform.parent = Holder.transform;
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