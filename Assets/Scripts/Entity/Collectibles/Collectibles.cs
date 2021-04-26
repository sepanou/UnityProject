using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.Collectibles {
	public class Collectibles: Entity {
		[ServerCallback]
		public static IEnumerator OnTargetDetected(Entity collectible, Player target, float speed = 5f) {
			while (collectible.Position - target.Position != Vector2.zero) {
				collectible.Position = Vector2.MoveTowards(
					collectible.Position,
					target.Position,
					speed * Time.deltaTime
				);
				yield return null;
			}
			target.RpcCollect(collectible.netId);
		}
	}
}