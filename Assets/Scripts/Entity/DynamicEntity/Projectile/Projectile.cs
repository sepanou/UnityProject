using Entity.DynamicEntity.Weapon.RangedWeapon;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	public abstract class Projectile: DynamicEntity {
		protected RangedWeapon FromWeapon;
		protected Rigidbody2D RigidBody;
		protected Vector2 FacingDirection;
		
		public new void Instantiate() {
			base.Instantiate();
			if (TryGetComponent(out RigidBody)) RigidBody.bodyType = netIdentity.isServer
				? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
		}
		
		protected abstract void Move();

		[ServerCallback]
		public static void SpawnProjectile(RangedWeapon source, Vector2 position) {
			Quaternion localRotation = Quaternion.Euler(
				new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, source.orientation))
			);
			Projectile projectile = Instantiate(source.GetProjectile(), position, localRotation);
			projectile.FromWeapon = source;
			projectile.FacingDirection = source.orientation;
			projectile.Instantiate();
			SetSameRenderingParameters(source, projectile);
			NetworkServer.Spawn(projectile.gameObject);
		}

		[ServerCallback]
		private void FixedUpdate() => Move();
	}
}
