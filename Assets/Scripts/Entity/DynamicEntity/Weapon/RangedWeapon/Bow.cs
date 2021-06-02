using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	public class Bow: RangedWeapon {
		public override void OnStartServer() {
			base.OnStartServer();
			Instantiate();
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (!isServer) Instantiate();
		}

		[ClientCallback] private void FixedUpdate() {
			if (!hasAuthority|| !equipped || isGrounded || !MouseCursor.Instance) return;
			// Only run by the weapon's owner (client)
			gameObject.transform.localRotation =
				MouseCursor.Instance.OrientateObjectTowardsMouse(Vector2.right, out Vector2 orient);
			CmdUpdateOrientation(orient);
		}

		[Server] protected override void DefaultAttack() {
			RpcAttackAnimation();
			Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
			LastAttackTime = Time.time;
		}

		[Server] protected override void SpecialAttack() {
			RpcAttackAnimation();
			Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
			holder.ReduceEnergy(specialAttackCost);
			LastAttackTime = Time.time;
		}

		[ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
		private void RpcAttackAnimation() => Animator.Play("DefaultAttack");
		
		[Command] // Authority does not change the fact that sync vars must be updated on the server
		private void CmdUpdateOrientation(Vector2 bowOrientation) => orientation = bowOrientation;
	}
}