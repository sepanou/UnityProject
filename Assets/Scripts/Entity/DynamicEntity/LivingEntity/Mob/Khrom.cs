using Behaviour;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Khrom : Mob {
        protected override void RpcDying() {
            NetworkServer.Destroy(gameObject);
            Debug.Log("Imma head out :(");
        }

        private void Start() {
            Instantiate();
            Behaviour = new NearestPlayerStraightFollower(this);
        }
    }
}