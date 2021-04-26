using UnityEngine;
using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob: LivingEntity {
		protected IBehaviour Behaviour = new Idle();

		private void FixedUpdate() {
			if (!isServer || Behaviour is null) return;
			Vector2 direction = Behaviour.NextDirection();
			RpcApplyForceToRigidBody(direction.x, direction.y);
		}
	}
}