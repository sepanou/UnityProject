using UnityEngine;
using Behaviour;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob: LivingEntity {
		protected IBehaviour Behaviour = new Idle();

		protected new void Instantiate() => base.Instantiate();

		[ServerCallback] private void FixedUpdate() {
			if (Behaviour is null) return;
			Vector2 direction = Behaviour.NextDirection();
			Move(direction.x, direction.y);
		}
	}
}