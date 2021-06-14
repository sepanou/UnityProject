﻿using Behaviour;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Wolf : Mob {
        protected override void RpcDying() {
            NetworkServer.Destroy(gameObject);
        }

        private void Start() {
            Instantiate();
            Behaviour = new NearestPlayerStraightFollower(this);
        }
    }
}