using System;
using System.Collections;
using Mirror;
using UI_Audio;
using UnityEngine;
using AnimationState = Entity.DynamicEntity.LivingEntity.AnimationState;

namespace Entity.DynamicEntity.Weapon.MeleeWeapon {
	[Serializable]
	public class MeleeWeaponData {
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

		private void Start() => Instantiate();

		public override RectTransform GetInformationPopup() {
			return !PlayerInfoManager.Instance 
				? null 
				: PlayerInfoManager.Instance.ShowMeleeWeaponDescription(meleeData);
		}

		public override string GetName() => meleeData.name;

		[ClientCallback] private void FixedUpdate() {
			if (!hasAuthority || !equipped || isGrounded || !MouseCursor.Instance) return;
			if (!_animating) SetLocalPosition();
		}

		[Server] protected override void DefaultAttack() {
			TargetProcessAttack(holder.connectionToClient);
			LastAttackTime = Time.time;
		}

		[Server] protected override void SpecialAttack() {
			TargetProcessAttack(holder.connectionToClient);
			holder.ReduceEnergy(specialAttackCost);
			LastAttackTime = Time.time;
		}

		[Client] private void SetLocalPosition() {
			if (!hasAuthority) return;
			Transform _transform = transform;
			switch (holder.LastAnimationState) {
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
			transform.rotation = Quaternion.identity;
		}

		[Command] private void CmdSetSpriteFlipX(bool state) => RpcSetSpriteFlipX(state);

		[ClientRpc] private void RpcSetSpriteFlipX(bool state) => spriteRenderer.flipX = state;

		[TargetRpc] private void TargetProcessAttack(NetworkConnection target) {
			StopAllCoroutines();
			StartCoroutine(ProcessAttack());
		}
	}
}
