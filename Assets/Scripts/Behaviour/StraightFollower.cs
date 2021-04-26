using Entity;
using Targeter;
using UnityEngine;

namespace Behaviour {
	public class StraightFollower<TTargeter>: EntityTargetedBehaviour<TTargeter> where TTargeter: ITargeter {
		public StraightFollower(Entity.Entity source, TTargeter targeter): base(source, targeter) { }
		
		public override Vector2 NextDirection() {
			Entity.Entity target = Targeter.AcquireTarget();
			return target is null ? Vector2.zero : target.Position - Entity.Position;
		}
	}
	
	public class NearestPlayerStraightFollower: StraightFollower<NearestPlayerTargeter> {
		public NearestPlayerStraightFollower(Entity.Entity source, NearestPlayerTargeter targeter): base(source, targeter) { }
	}
	
	public class SpecificStraightFollower<TEntity>: StraightFollower<SpecificTargeter<TEntity>> where TEntity: Entity.Entity {
		public SpecificStraightFollower(Entity.Entity source, TEntity target): base(source, new SpecificTargeter<TEntity>(target)) { }
	}
	
	public class SpecificMarkerStraightFollower: SpecificStraightFollower<Marker> {
		public SpecificMarkerStraightFollower(Entity.Entity source, Marker target): base(source, target) { }
	}
}