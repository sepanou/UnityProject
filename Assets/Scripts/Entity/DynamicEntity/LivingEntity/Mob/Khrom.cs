using Behaviour;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Khrom: Mob {
        public override int cooldown { get; protected set; } = 180;
        public override int atk { get; protected set; } = 50;
        private const float AtkMaxDist = 128;

        private void Start() {
            Instantiate();
        }

        protected override bool CanAttack() {
            Player.Player target = ((DistanceNearestPlayerStraightFollower)behaviour).behaviour.targeter.target;
            return target && (target.transform.position - transform.position).sqrMagnitude < AtkMaxDist * AtkMaxDist;
        }

        protected override void Attack() {
            Player.Player target = ((DistanceNearestPlayerStraightFollower)behaviour).behaviour.targeter.target;
            Animator.SetBool(isAttacking, true);
            target.GetAttacked(atk);
            target.Animator.SetTrigger(hasBeenHit);
        }
    }
}