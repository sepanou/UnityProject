using Behaviour;
using Behaviour.Targeter;
using Mirror;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public class ForestSpirit: Mob {
		public override int cooldown { get; protected set; } = 50;
		public override int atk { get; protected set; } = 20;
		private const float AtkMaxDist = 12;
		private const float MovMinDist = 4;
		private const float MovMaxDist = 16;

		private void Start() {
			Instantiate();
			behaviour = new DistanceNearestPlayerStraightFollower(this, MovMinDist, MovMaxDist);
		}

		protected override bool CanAttack() {
			Player.Player target = null;
			switch (behaviour) {
				case DistanceNearestPlayerStraightFollower distanceNearestPlayerStraightFollower:
					target = distanceNearestPlayerStraightFollower.behaviour.targeter.target;
					break;
				case EntityTargetedBehaviour<NearestPlayerTargeter> nearestPlayerBehaviour:
					target = nearestPlayerBehaviour.targeter.target;
					break;
			}
			return target && (target.transform.position - transform.position).sqrMagnitude < AtkMaxDist * AtkMaxDist;
		}

		[Server] protected override void Attack() {
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
			
			if (IsAttacking is null)
				IsAttacking = StartCoroutine(AttackAnimation());
			Projectile.Projectile.BuildMobProjectile(WeaponGenerator.GetStaffProjectile(), this, (target.transform.position - transform.position).normalized);
		}
	}
}