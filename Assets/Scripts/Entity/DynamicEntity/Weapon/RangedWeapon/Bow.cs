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
			if (!NetworkClient.ready || !hasAuthority|| !Equipped || IsGrounded || !MouseCursor.Instance) return;
			// Only run by the weapon's owner (client)
			gameObject.transform.localRotation =
				MouseCursor.Instance.OrientateObjectTowardsMouse(Vector2.right, out Vector2 orient);
			CmdUpdateOrientation(orient);
		}
		
		[Server] protected override void SetProjectile() => Projectile = WeaponGenerator.GetBowProjectile();

		[Server] protected override void DefaultAttack() {
			global::Entity.DynamicEntity.Projectile.Projectile.SpawnProjectiles(this, launchPoint.position, false);
		}

		[Server] protected override void SpecialAttack() {
			global::Entity.DynamicEntity.Projectile.Projectile.SpawnProjectiles(this, launchPoint.position, true);
		}

		[Command] // Authority does not change the fact that sync vars must be updated on the server
		private void CmdUpdateOrientation(Vector2 bowOrientation) => orientation = bowOrientation;
	}
}