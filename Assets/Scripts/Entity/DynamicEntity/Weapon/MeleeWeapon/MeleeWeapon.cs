using System;
using System.Collections;
using Entity.DynamicEntity.LivingEntity.Mob;
using Mirror;
using UI_Audio;
using UnityEngine;
using AnimationState = Entity.DynamicEntity.LivingEntity.AnimationState;

namespace Entity.DynamicEntity.Weapon.MeleeWeapon {
	[Serializable]
	public class MeleeWeaponData {
		public const float MaxKnockbackMultiplier = 3f,
			MaxWeaponSizeMultiplier = 2f,
			MaxDefaultDamageMultiplier = 3f,
			MaxSpecialDamageMultiplier = 1.5f,
			MinKnockbackMultiplier = 0.75f,
			MinWeaponSizeMultiplier = 0.75f,
			MinDefaultDamageMultiplier = 0.5f,
			MinSpecialDamageMultiplier = 0.75f;
		
		public float knockbackMultiplier, weaponSizeMultiplier;
		public float defaultDamageMultiplier, specialDamageMultiplier;
		public string name;

		public static MeleeWeaponData operator *(MeleeWeaponData other, int nbr) {
			if (other == null || nbr == 0)
				return null;
			if (nbr == 1)
				return other;
			return new MeleeWeaponData {
				knockbackMultiplier = other.knockbackMultiplier * nbr,
				weaponSizeMultiplier = other.weaponSizeMultiplier * nbr,
				defaultDamageMultiplier = other.defaultDamageMultiplier * nbr,
				specialDamageMultiplier = other.specialDamageMultiplier * nbr
			};
		}

		public static int GetKibryValue(MeleeWeaponData data) {
			float kibryValue = 0f;
			kibryValue += 100f * (data.knockbackMultiplier - MinKnockbackMultiplier) /
			              (MaxKnockbackMultiplier - MinKnockbackMultiplier);
			kibryValue += 100f * (data.weaponSizeMultiplier - MinWeaponSizeMultiplier) /
			              (MaxWeaponSizeMultiplier - MinWeaponSizeMultiplier);
			kibryValue += 100f * (data.defaultDamageMultiplier - MinDefaultDamageMultiplier) /
			              (MaxDefaultDamageMultiplier - MinDefaultDamageMultiplier);
			kibryValue += 100f * (data.specialDamageMultiplier - MinSpecialDamageMultiplier) /
			              (MaxSpecialDamageMultiplier - MinSpecialDamageMultiplier);
			return (int) kibryValue;
		}
	}
	
	public class MeleeWeapon: Weapon {
		[SyncVar] [HideInInspector] public MeleeWeaponData meleeData;
		private bool _animating;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.Write(meleeData);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			meleeData = reader.Read<MeleeWeaponData>();
		}

		public override void OnStartServer() {
			base.OnStartServer();
			transform.localScale *= meleeData.weaponSizeMultiplier;
			Instantiate();
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (!isServer) Instantiate();
		}

		public override void OnStopAuthority() {
			StopAllCoroutines();
			base.OnStopAuthority();
		}

		protected override float GetDamageMultiplier(bool isSpecial) =>
			isSpecial ? meleeData.specialDamageMultiplier : meleeData.defaultDamageMultiplier;

		public override RectTransform GetInformationPopup() 
			=> PlayerInfoManager.ShowMeleeWeaponDescription(meleeData);

		public override int GetKibryValue() => MeleeWeaponData.GetKibryValue(meleeData);

		public override string GetWeaponName() => meleeData.name;

		[ClientCallback] private void FixedUpdate() {
			if (!NetworkClient.ready || !hasAuthority || !Equipped || IsGrounded || !MouseCursor.Instance) return;
			if (!_animating) SetLocalPosition();
		}

		[Server] protected override void DefaultAttack() {
			TargetProcessAttack(Holder.connectionToClient);
		}

		[Server] protected override void SpecialAttack() {
			TargetProcessAttack(Holder.connectionToClient);
		}

		[Client] private void SetLocalPosition() {
			if (!hasAuthority) return;
			Transform _transform = transform;
			switch (Holder.LastAnimationState) {
				case AnimationState.East:
				case AnimationState.North:
					_transform.localPosition = new Vector2(0.3f, _transform.localPosition.y);
					CmdSetSpriteFlipX(false);
					break;
				default:
					_transform.localPosition = new Vector2(-0.3f, _transform.localPosition.y);
					CmdSetSpriteFlipX(true);
					break;
			}
		}

		[Client] // Only run by the owner -> networkTransform automatically synchronizes everything
		private IEnumerator ProcessAttack() {
			SetLocalPosition();
			_animating = true;
			CmdSetAnimating(true);
			MouseCursor.Instance.OrientateObjectTowardsMouse(Vector3.up, out Vector2 orientation);
			int targetAngle = (int) Vector2.SignedAngle(Vector2.up, orientation);
			int signX = transform.localPosition.x < 0f ? -1 : 1;
			int signZ = targetAngle < 0 ? -1 : 1;
			CmdSetSpriteFlipX(signZ == 1);

			for (int k = 1; k < 7; k++) {
				yield return new WaitForSeconds(0.01f);
				transform.localPosition -= new Vector3(signX * 0.05f, 0, 0);
				transform.rotation = Quaternion.Euler(0, 0, -signZ * k * 7.5f);
			}
			
			targetAngle += signZ * 75;

			for (int x = targetAngle / 5; x != 0; x -= signZ) {
				transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + signZ * 5);
				yield return new WaitForSeconds(0.01f);
			}
			
			_animating = false;
			CmdSetAnimating(false);
			transform.rotation = Quaternion.identity;
		}

		[Command] private void CmdSetSpriteFlipX(bool state) => RpcSetSpriteFlipX(state);

		[Command] private void CmdSetAnimating(bool state) => _animating = state;

		[ClientRpc] private void RpcSetSpriteFlipX(bool state) => spriteRenderer.flipX = state;

		[TargetRpc] private void TargetProcessAttack(NetworkConnection target) {
			StopAllCoroutines();
			StartCoroutine(ProcessAttack());
		}
		
		protected override void OnTriggerEnter2D(Collider2D other) {
			if (isClient)
				base.OnTriggerEnter2D(other); // Only useful for interactions (client side)
			
			if (!isServer || !_animating || !other.gameObject.TryGetComponent(out Mob mob)) return;
			// Server code
			mob.GetAttacked(GetDamage());
			mob.TakeKnockBack(transform.position, meleeData.knockbackMultiplier);
		}
	}
}
