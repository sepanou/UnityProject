using System.Collections;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.Collectibles {
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(Collider2D))]
	public class Collectibles: Entity {
		[Server] public static IEnumerator OnTargetDetected(Entity collectible, Player target, float speed = 5f) {
			while (collectible.Position - target.Position != Vector2.zero) {
				collectible.Position = Vector2.MoveTowards(
					collectible.Position,
					target.Position,
					speed * Time.deltaTime
				);
				yield return null;
			}
			target.Collect(collectible.netId);
		}
	}
}