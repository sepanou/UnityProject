using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob: LivingEntity {
		protected Behaviour.IBehaviour Behaviour = null;
		
		protected new void Instantiate() {
			base.Instantiate();
		}

		[ServerCallback]
		private void FixedUpdate() {
			if (Behaviour is null) return;
			Vector2 direction = Behaviour.NextDirection();
			RpcApplyForceToRigidBody(direction.x, direction.y);
		}
	}
}