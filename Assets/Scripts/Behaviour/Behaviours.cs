using UnityEngine;

namespace Behaviour {
	public interface IBehaviour {
		Vector2 NextDirection();
	}
	
	public abstract class Behaviour<TEntity>: IBehaviour where TEntity: Entity.Entity {
		protected readonly TEntity Entity;

		protected Behaviour(TEntity entity) {
			Entity = entity;
		}

		public abstract Vector2 NextDirection();
	}
	
	public abstract class EntityBehaviour: Behaviour<Entity.Entity> {
		protected EntityBehaviour(Entity.Entity entity): base(entity) { }
	}
	
	public abstract class TargetedBehaviour<TEntity, TTarget>: Behaviour<TEntity> where TEntity: Entity.Entity where TTarget: Entity.Entity {
		protected readonly TTarget Target;

		protected TargetedBehaviour(TEntity entity, TTarget target): base(entity) {
			Target = target;
		}
	}
	
	public abstract class EntityTargetedBehaviour: TargetedBehaviour<Entity.Entity, Entity.Entity> {
		protected EntityTargetedBehaviour(Entity.Entity entity, Entity.Entity target): base(entity, target) { }
	}
}