using System;
using System.Collections;
using UnityEngine;
using Behaviour;
using Behaviour.Targeter;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob : LivingEntity {
		protected IBehaviour behaviour = new Idle();
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
			return target && target.Collider2D.IsTouching(Collider2D);
		}

		protected virtual void Attack() {
			Player.Player target;
			switch (behaviour) {
				case DistanceNearestPlayerStraightFollower distanceNearestPlayerStraightFollower:
					target = distanceNearestPlayerStraightFollower.behaviour.targeter.target;
					break;
				case EntityTargetedBehaviour<NearestPlayerTargeter> nearestPlayerBehaviour:
					target = nearestPlayerBehaviour.targeter.target;
					break;
				default:
					return;
			}

			Animator.Play(AttackAnims[(int) LastAnimationState]);
			target.GetAttacked(atk);
		}

		private IEnumerator DeathAnimation() {
			behaviour = new Idle();
			Animator.SetTrigger(IsDeadId);
			yield return new WaitForSeconds(0.3f);
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