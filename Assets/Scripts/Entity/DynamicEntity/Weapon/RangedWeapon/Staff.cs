using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	public class Staff: RangedWeapon {
		public override void OnStartServer() {
			base.OnStartServer();
			Instantiate();
		}

		public override void OnStartClient() {
			base.OnStartClient();
			if (!isServer) Instantiate();
		}

		[ClientCallback] private void FixedUpdate() {
			if (!NetworkClient.ready || !hasAuthority|| !Equipped || IsGrounded || !MouseCursor.Instance) return;
			// Only run by the weapon's owner (client)
			gameObject.transform.localRotation =
				MouseCursor.Instance.OrientateObjectTowardsMouse(Vector2.up, out Vector2 orient);
			CmdUpdateOrientation(orient);
		}
		
		[Server] protected override void SetProjectile() => Projectile = WeaponGenerator.GetStaffProjectile();

		[Server] protected override void DefaultAttack() {
			RpcAttackAnimation(false);
			global::Entity.DynamicEntity.Projectile.Projectile.SpawnProjectiles(this, launchPoint.position, false);
		}

		[Server] protected override void SpecialAttack() {
			RpcAttackAnimation(true);
			global::Entity.DynamicEntity.Projectile.Projectile.SpawnProjectiles(this, launchPoint.position, true);
		}

		[ClientRpc] // By default, attack anims are fast -> no need for persistent NetworkAnimator
		private void RpcAttackAnimation(bool specialAttack)
			=> Animator.Play(specialAttack ? "SpecialAttack" : "DefaultAttack");
		
		[Command] // Authority does not change the fact that sync vars must be updated on the server
		private void CmdUpdateOrientation(Vector2 staffOrientation) => orientation = staffOrientation;
	}
}
