using Behaviour.Targeter;
using Entity;
using UnityEngine;

namespace Behaviour {
	public class StraightFollower<TTargeter>: EntityTargetedBehaviour<TTargeter> where TTargeter: ITargeter {
		public StraightFollower(Entity.Entity source, TTargeter targeter): base(source, targeter) { }
		
		public override Vector2 NextDirection() {
			Entity.Entity target = targeter.AcquireTarget();
			return target is null ? Vector2.zero : target.Position - entity.Position;
		}
	}
	
	public class NearestPlayerStraightFollower: StraightFollower<NearestPlayerTargeter> {
		public NearestPlayerStraightFollower(Entity.Entity source): base(source, new NearestPlayerTargeter(source)) { }
	}
	
	public class SpecificStraightFollower<TEntity>: StraightFollower<SpecificTargeter<TEntity>> where TEntity: Entity.Entity {
		public SpecificStraightFollower(Entity.Entity source, TEntity target): base(source, new SpecificTargeter<TEntity>(target)) { }
	}
	
	public class SpecificMarkerStraightFollower: SpecificStraightFollower<Marker> {
		public SpecificMarkerStraightFollower(Entity.Entity source, Marker target): base(source, target) { }
	}
}