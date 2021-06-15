using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Bat: Mob {
        public override int cooldown { get; protected set; } = 60;
        public override int atk { get; protected set; } = 5;
        private const float MovMinDist = 0.25f;
        private const float MovMaxDist = 8;

        private void Start() {
            Instantiate();
            behaviour = new DistanceNearestPlayerStraightFollower(this, MovMinDist, MovMaxDist);
        }
    }
}