using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	[RequireComponent(typeof(Rigidbody2D))]
	public class Arrow: Projectile {
		private void Start() => Instantiate();

		[Server] protected override void Move() {
			if (RigidBody.velocity != Vector2.zero) return;
			RigidBody.velocity = FacingDirection * Speed;
		}
	}
}