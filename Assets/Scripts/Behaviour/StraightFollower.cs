using Behaviour.Targeter;
using UnityEngine;

namespace Behaviour {
	public class StraightFollower<TTargeter>: EntityTargetedBehaviour<TTargeter> where TTargeter: ITargeter {
		public StraightFollower(Entity.Entity source, TTargeter targeter): base(source, targeter) { }
		
		public override Vector2 NextDirection() {
			Entity.Entity target = targeter.AcquireTarget();
			return target ? target.Position - entity.Position : Vector2.zero;
		}
	}
	
	public class NearestPlayerStraightFollower: StraightFollower<NearestPlayerTargeter> {
		public NearestPlayerStraightFollower(Entity.Entity source): base(source, new NearestPlayerTargeter(source)) { }
	}
	
	public class SpecificStraightFollower<TEntity>: StraightFollower<SpecificTargeter<TEntity>> where TEntity: Entity.Entity {
		public SpecificStraightFollower(Entity.Entity source, TEntity target): base(source, new SpecificTargeter<TEntity>(target)) { }
	}
}