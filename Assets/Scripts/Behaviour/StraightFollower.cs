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
	
	public class PlayerStraightFollower: StraightFollower<PlayerTargeter> {
		public PlayerStraightFollower(Entity.Entity source, PlayerTargeter targeter): base(source, targeter) { }
	}
}