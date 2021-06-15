using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public class Cow: Mob {
		public override int cooldown { get; protected set; } = 60;
		public override int atk { get; protected set; } = 20;
		private const float MovMinDist = 0.25f;
		private const float MovMaxDist = 16;

		private void Start() {
			Instantiate();
			behaviour = new DistanceNearestPlayerStraightFollower(this, MovMinDist, MovMaxDist);
		}
	}
}