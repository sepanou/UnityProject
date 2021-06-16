using System.Collections;
using UnityEngine;
using Behaviour;
using Behaviour.Targeter;
using Entity.Collectibles;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public abstract class Mob : LivingEntity {
		protected IBehaviour behaviour = new Idle();
		public abstract int cooldown { get; protected set; }
		protected int cooldownCount = 0;
		public abstract int atk { get; protected set; }
		protected Coroutine IsAttacking;

		public override void OnStartServer() {
			OnEntityDie.AddListener(OnDeath);
			base.OnStartServer();
		}

		[Server] private void OnDeath(LivingEntity living) {
			StartCoroutine(DeathAnimation());
			CustomNetworkManager.Instance.AlivePlayers.ForEach(p => p.AddHealth(5));
			Kibry kibry = WeaponGenerator.GetRandomKibry();
			kibry.transform.position = living.Position;
			NetworkServer.Spawn(kibry.gameObject);
		}

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

		[Server] protected virtual void Attack() {
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
			
			// Don't change to the funny operator (not supported)
			if (IsAttacking is null)
				IsAttacking = StartCoroutine(AttackAnimation());
			target.GetAttacked(atk);
		}

		[ClientRpc] private void RpcPlayAttackAnimation(int state) => Animator.Play(AttackAnims[state]);
		
		[ClientRpc] protected override void RpcDying() => Animator.SetTrigger(IsDeadId);

		[Server] protected IEnumerator AttackAnimation() {
			IBehaviour temp = behaviour;
			behaviour = new Idle();
			RpcPlayAttackAnimation((int) LastAnimationState);
			yield return new WaitForSeconds(0.3f);
			behaviour = temp;
			IsAttacking = null;
		}

		[Server] private IEnumerator DeathAnimation() {
			behaviour = new Idle();
			Animator.SetTrigger(IsDeadId);
			yield return new WaitForSeconds(0.5f);
			NetworkServer.Destroy(gameObject);
		}

		[ServerCallback]
		private void FixedUpdate() {
			if (CanAttack()) {
				if (cooldownCount == 0) {
					Attack();
				}
				cooldownCount = (cooldownCount + 1) % cooldown;
			}
			if (behaviour is null) return;
			Vector2 direction = behaviour.NextDirection();
			if (direction == Vector2.zero) return;
			Move(direction.x, direction.y);
		}
	}
}