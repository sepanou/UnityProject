using Behaviour.Targeter;

namespace Entity.DynamicEntity.LivingEntity.Mob {
    public class Khrom: Mob {
        public override int cooldown { get; protected set; } = 180;
        public override int atk { get; protected set; } = 50;
        private const float AtkMaxDist = 128;
        private NearestPlayerTargeter targeter;

        private void Start() {
            Instantiate();
            targeter = new NearestPlayerTargeter(this);
        }

        protected override bool CanAttack() {
            Player.Player target = (Player.Player)targeter.AcquireTarget();
            return target && (target.transform.position - transform.position).sqrMagnitude < AtkMaxDist * AtkMaxDist;
        }

        protected override void Attack() {
            Player.Player target = targeter.target;
            Animator.Play(AttackAnims[(int) LastAnimationState]);
            Projectile.Projectile.BuildMobProjectile(WeaponGenerator.GetStaffProjectile(), this, (target.transform.position - transform.position).normalized);
        }
    }
}