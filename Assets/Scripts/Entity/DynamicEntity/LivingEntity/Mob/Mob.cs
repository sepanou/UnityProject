using System.Collections;
using UnityEngine;
using Behaviour;
using Behaviour.Targeter;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob : LivingEntity {
		protected IBehaviour behaviour = new Idle();
		protected static readonly int hasBeenHit = Animator.StringToHash("HasBeenHit");
		protected static readonly int isAttacking = Animator.StringToHash("IsAttacking");
		protected static readonly int isDead = Animator.StringToHash("IsDead");
		public abstract int cooldown { get; protected set; }
		protected int cooldownCount = 0;
		public abstract int atk { get; protected set; }

		protected virtual bool CanAttack() {
			Player.Player target = null;
			switch (behaviour) {
				case DistanceNearestPlayerStraightFollower distanceNearestPlayerStraightFollower:
					target = distanceNearestPlayerStraightFollower.behaviour.targeter.target;
					break;
				case EntityTargetedBehaviour<NearestPlayerTargeter> nearestPlayerBehaviour:
					target = nearestPlayerBehaviour.targeter.target;
					break;
			}
			return target && target.Collider2D.IsTouching(Collider2D) && !target.Animator.GetBool(hasBeenHit);
		}

		protected virtual void Attack() {
			if (!(behaviour is DistanceNearestPlayerStraightFollower distanceNearestPlayerStraightFollower)) return;
			Player.Player target = distanceNearestPlayerStraightFollower.behaviour.targeter.target;
			Animator.SetBool(isAttacking, true);
			target.GetAttacked(atk);
			target.Animator.SetTrigger(hasBeenHit);
		}

		private IEnumerator DeathAnimation() {
			behaviour = new Idle();
			Animator.SetBool(isDead, true);
			yield return new WaitUntil(() => !Animator.GetBool(isDead));
			NetworkServer.Destroy(gameObject);
		}
		
		protected override void RpcDying() {
			StartCoroutine(DeathAnimation());
		}

		[ServerCallback]
		private void FixedUpdate() {
			if (CanAttack()) {
				if (cooldownCount == 0)
					Attack();
				cooldownCount = (cooldownCount + 1) % cooldown;
			}
			if (behaviour is null) return;
			Vector2 direction = behaviour.NextDirection();
			Move(direction.x, direction.y);
		}
	}
}