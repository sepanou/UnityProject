using Behaviour;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Wolf : Mob {
        public override int cooldown { get; protected set; } = 30;
        public override int atk { get; protected set; } = 10;
        private const float MovMinDist = 0.25f;
        private const float MovMaxDist = 32;

        private void Start() {
            Instantiate();
            behaviour = new DistanceNearestPlayerStraightFollower(this, MovMinDist, MovMaxDist);
        }
    }
}