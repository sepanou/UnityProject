using Entity.DynamicEntity.LivingEntity;
using UnityEngine;

namespace Behaviour {
	public class DistanceBehaviour<TEntity, TBehaviour>: Behaviour<TEntity> where TEntity: Entity.Entity where TBehaviour: Behaviour<TEntity> {
		public readonly TBehaviour behaviour;
		protected readonly float sqrMin;
		protected readonly float sqrMax;
		public float SqrDistance { get; private set; }

		public DistanceBehaviour(TBehaviour behaviour, float min, float max): base(behaviour.entity) {
			this.behaviour = behaviour;
			sqrMin = min * min;
			sqrMax = max * max;
		}
		
		public override Vector2 NextDirection() {
			Vector2 direction = behaviour.NextDirection(); 
			SqrDistance = direction.sqrMagnitude;
			return sqrMin <= SqrDistance && SqrDistance <= sqrMax ? direction : Vector2.zero;
		}
	}

	public class EntityDistanceBehaviour<TBehaviour>: DistanceBehaviour<Entity.Entity, TBehaviour> where TBehaviour: Behaviour<Entity.Entity> {
		public EntityDistanceBehaviour(TBehaviour behaviour, float min, float max): base(behaviour, min, max) { }
	}
	
	public class DistanceNearestPlayerStraightFollower: EntityDistanceBehaviour<NearestPlayerStraightFollower> {
		public DistanceNearestPlayerStraightFollower(Entity.Entity entity, float min, float max): base(new NearestPlayerStraightFollower(entity), min, max) { }
	}
}