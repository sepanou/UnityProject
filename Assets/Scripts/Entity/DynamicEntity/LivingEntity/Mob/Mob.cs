using UnityEngine;
using Behaviour;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob: LivingEntity {
		protected IBehaviour behaviour = new Idle();
		
		protected override void RpcDying() {
			NetworkServer.Destroy(gameObject);
		}

		[ServerCallback]
		private void FixedUpdate() {
			if (behaviour is null) return;
			Vector2 direction = behaviour.NextDirection();
			Move(direction.x, direction.y);
		}
	}
}