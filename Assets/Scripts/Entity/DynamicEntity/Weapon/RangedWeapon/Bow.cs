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
		
		[Server] protected override void SetProjectile() => ProjectilePrefab = WeaponGenerator.GetBowProjectile();

		[Server] protected override void DefaultAttack() {
			StopAllCoroutines();
			StartCoroutine(Projectile.Projectile.SpawnProjectiles(this, launchPoint));
		}

		[Server] protected override void SpecialAttack() {
			StopAllCoroutines();
			StartCoroutine(Projectile.Projectile.WaveOfProjectiles(this, launchPoint, 5f));
		}

		[Command] // Authority does not change the fact that sync vars must be updated on the server
		private void CmdUpdateOrientation(Vector2 bowOrientation) => orientation = bowOrientation;
	}
}