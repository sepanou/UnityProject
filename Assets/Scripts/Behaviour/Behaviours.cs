using Entity.DynamicEntity.LivingEntity.Player;
using Targeter;
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
	
	public abstract class TargetedBehaviour<TEntity, TTargeter>: Behaviour<TEntity> where TEntity: Entity.Entity where TTargeter: ITargeter {
		protected readonly TTargeter Targeter;

		protected TargetedBehaviour(TEntity entity, TTargeter targeter): base(entity) {
			Targeter = targeter;
		}
	}
	
	public abstract class EntityTargetedBehaviour<TTargeter>: TargetedBehaviour<Entity.Entity, TTargeter> where TTargeter: ITargeter {
		protected EntityTargetedBehaviour(Entity.Entity entity, TTargeter targeter): base(entity, targeter) { }
	}
	
	public abstract class EntityPlayerTargetedBehaviour: EntityTargetedBehaviour<PlayerTargeter> {
		protected EntityPlayerTargetedBehaviour(Entity.Entity entity, PlayerTargeter targeter): base(entity, targeter) { }
	}
}