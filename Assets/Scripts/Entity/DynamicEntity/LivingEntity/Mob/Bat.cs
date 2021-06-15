using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Bat: Mob {
        private void Start() {
            Instantiate();
            behaviour = new NearestPlayerStraightFollower(this);
        }
    }
}