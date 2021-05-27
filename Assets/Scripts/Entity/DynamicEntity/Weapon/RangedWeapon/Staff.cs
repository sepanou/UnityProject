using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	public class Staff: RangedWeapon {
		private void Start() => Instantiate();

		[ClientCallback] private void FixedUpdate() {
			if (!hasAuthority|| !equipped || isGrounded || !MouseCursor.Instance) return;
			// Only run by the weapon's owner (client)
			gameObject.transform.localRotation =
				MouseCursor.Instance.OrientateObjectTowardsMouse(Vector2.up, out Vector2 orient);
			CmdUpdateOrientation(orient);
		}

		[Server] protected override void DefaultAttack() {
			RpcAttackAnimation(false);
			Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
			LastAttackTime = Time.time;
		}

		[Server] protected override void SpecialAttack() {
			RpcAttackAnimation(true);
			Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
			holder.ReduceEnergy(specialAttackCost);
			LastAttackTime = Time.time;
		}

		[ClientRpc] // By default, attack anims are fast -> no need for persistent NetworkAnimator
		private void RpcAttackAnimation(bool specialAttack)
			=> Animator.Play(specialAttack ? "SpecialAttack" : "DefaultAttack");
		
		[Command] // Authority does not change the fact that sync vars must be updated on the server
		private void CmdUpdateOrientation(Vector2 staffOrientation) => orientation = staffOrientation;
	}
}
