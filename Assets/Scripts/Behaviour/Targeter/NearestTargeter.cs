using System;
using Entity.DynamicEntity.LivingEntity.Player;

namespace Targeter {
	public class NearestTargeter<TEntity, TTarget>: ITargeter where TEntity: Entity.Entity where TTarget: Entity.Entity {
		protected readonly TEntity Entity;
		protected readonly Func<TEntity, TTarget, TTarget, bool> IsNearer;
		
		public NearestTargeter(TEntity entity, Func<TEntity, TTarget, TTarget, bool> isNearer = null) {
			Entity = entity;
			IsNearer = isNearer
				?? ((source, nearest, other) => !(nearest is null) && (source.Position - nearest.Position).sqrMagnitude <= (source.Position - other.Position).sqrMagnitude);
		}

		public Entity.Entity AcquireTarget() {
			TTarget[] entities = UnityEngine.Object.FindObjectsOfType<TTarget>();
			TTarget nearest = null;
			foreach (TTarget entity in entities) {
				if (IsNearer(Entity, nearest, entity)) continue;
				nearest = entity;
			}
			return nearest;
		}
	}
	
	public class NearestPlayerTargeter: NearestTargeter<Entity.Entity, Player> {
		public NearestPlayerTargeter(Entity.Entity entity, Func<Entity.Entity, Player, Player, bool> isNearer = null): base(entity, isNearer) { }
	}
}