using UnityEngine;
using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob: LivingEntity {
		protected IBehaviour Behaviour = new Idle();

		protected new void Instantiate() => base.Instantiate();

		private void FixedUpdate() {
			if (!isServer || Behaviour is null) return;
			Vector2 direction = Behaviour.NextDirection();
			ApplyForceToRigidBody(direction.x, direction.y);
			RpcApplyForceToRigidBody(direction.x, direction.y);
		}
	}
}