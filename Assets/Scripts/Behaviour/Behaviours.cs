using Behaviour.Targeter;
using UnityEngine;

namespace Behaviour {
	public interface IBehaviour {
		Vector2 NextDirection();
	}
	
	public abstract class Behaviour<TEntity>: IBehaviour where TEntity: Entity.Entity {
		public readonly TEntity entity;

		protected Behaviour(TEntity entity) {
			this.entity = entity;
		}

		public abstract Vector2 NextDirection();
	}
	
	public abstract class EntityBehaviour: Behaviour<Entity.Entity> {
		protected EntityBehaviour(Entity.Entity entity): base(entity) { }
	}
	
	public abstract class TargetedBehaviour<TEntity, TTargeter>: Behaviour<TEntity> where TEntity: Entity.Entity where TTargeter: ITargeter {
		public readonly TTargeter targeter;

		protected TargetedBehaviour(TEntity entity, TTargeter targeter): base(entity) {
			this.targeter = targeter;
		}
	}
	
	public abstract class EntityTargetedBehaviour<TTargeter>: TargetedBehaviour<Entity.Entity, TTargeter> where TTargeter: ITargeter {
		protected EntityTargetedBehaviour(Entity.Entity entity, TTargeter targeter): base(entity, targeter) { }
	}
}