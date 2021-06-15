using Behaviour;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class ForestSpirit : Mob {
        protected override void RpcDying() {
            NetworkServer.Destroy(gameObject);
        }

        private void Start() {
            Instantiate();
            behaviour = new NearestPlayerStraightFollower(this);
        }
    }
}