﻿using Behaviour;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Slime : Mob {
        protected override void RpcDying() {
            NetworkServer.Destroy(gameObject);
            Debug.Log("Imma head out :(");
        }

        private void Start() {
            Instantiate();
            behaviour = new NearestPlayerStraightFollower(this);
        }
    }
}