using UnityEngine;

namespace Behaviour {
	public class StraightFollowing: EntityTargetedBehaviour {
		public StraightFollowing(Entity.Entity source,  Entity.Entity target): base(source, target) { }
		
		public override Vector2 NextDirection()
			=> Target.Position - Entity.Position;
	}
}