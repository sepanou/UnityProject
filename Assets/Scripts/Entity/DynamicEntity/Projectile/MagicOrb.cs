using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	public class MagicOrb: Projectile {
		private void Start() => Instantiate();

		[Server] protected override void Move() {
			if (RigidBody.velocity != Vector2.zero) return;
			RigidBody.velocity = FacingDirection * Speed;
			// Add specific behaviour here -> Perlin noise?
		}

		[ServerCallback] private void OnCollisionEnter2D(Collision2D other) {
			if (other.gameObject.TryGetComponent(out LivingEntity.LivingEntity entity))
				entity.GetAttacked(FromWeapon.GetDamage());
			NetworkServer.Destroy(gameObject);
		}
	}
}