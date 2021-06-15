using System;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;

namespace Behaviour.Targeter {
	public class NearestTargeter<TEntity, TTarget>: ITargeter where TEntity: Entity.Entity where TTarget: Entity.Entity {
		public readonly TEntity entity;
		public TTarget target { get; protected set; }
		public readonly Func<TEntity, TTarget, TTarget, bool> isNearer;
		
		public NearestTargeter(TEntity entity, Func<TEntity, TTarget, TTarget, bool> isNearer = null) {
			this.entity = entity;
			this.isNearer = isNearer ?? ((source, nearest, other) => !(nearest is null) && (source.Position - nearest.Position).sqrMagnitude <= (source.Position - other.Position).sqrMagnitude);
		}

		public virtual Entity.Entity AcquireTarget() {
			TTarget[] entities = UnityEngine.Object.FindObjectsOfType<TTarget>();
			target = null;
			foreach (TTarget targetEntity in entities) {
				if (!targetEntity || isNearer(entity, target, targetEntity)) continue;
				target = targetEntity;
			}
			return target;
		}
	}
	
	public class NearestPlayerTargeter: NearestTargeter<Entity.Entity, Player> {
		public NearestPlayerTargeter(Entity.Entity entity, Func<Entity.Entity, Player, Player, bool> isNearer = null): base(entity, isNearer) { }
		
		public override Entity.Entity AcquireTarget() {
			List<Player> entities = CustomNetworkManager.Instance.AlivePlayers;
			target = null;
			foreach (Player targetEntity in entities) {
				if (!targetEntity || !targetEntity.IsAlive || isNearer(entity, target, targetEntity)) continue;
				target = targetEntity;
			}
			return target;
		}
	}
}