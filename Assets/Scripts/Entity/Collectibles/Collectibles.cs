using System;
using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.Collectibles
{
    public class Collectibles : Entity
    {
        [ServerCallback]
        public static IEnumerator OnTargetDetected(Entity collectible, Player target, float speed = 5f)
        {
            while (collectible.GetPosition2D() - target.GetPosition2D() != Vector2.zero)
            {
                collectible.SetPosition2D(Vector2.MoveTowards(collectible.GetPosition2D(),
                    target.GetPosition2D(), speed * Time.deltaTime));
                yield return null;
            }
            target.RpcCollect(collectible.netId);
        }
    }
}