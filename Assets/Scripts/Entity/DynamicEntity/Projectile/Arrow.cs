using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	[RequireComponent(typeof(Rigidbody2D))]
	public class Arrow: Projectile {
		private void Start() {
			Instantiate();
		}

		[ServerCallback]
		protected override void Move() {
			if (RigidBody.velocity != Vector2.zero) return;
			RigidBody.velocity = FacingDirection * Speed;
		}

		[ServerCallback]
		private void OnCollisionEnter2D(Collision2D other) {
			if (other.gameObject.TryGetComponent(out LivingEntity.LivingEntity entity))
				entity.GetAttacked(FromWeapon.GetDamage());
			NetworkServer.Destroy(gameObject);
		}
	}
}