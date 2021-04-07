using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public class Bow : RangedWeapon
    {
        private void OrientateBow(Vector3 bowOrientation)
        {
            if (!hasAuthority) return;
            bowOrientation.Normalize();
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, bowOrientation)));
            CmdUpdateOrientation(bowOrientation);
        }

        private void FixedUpdate()
        {
            // Only run by server
            if (isServer && isGrounded && !PlayerFound) GroundedLogic();
            if (!hasAuthority|| !equipped || isGrounded) return;
            // Only run by the weapon's owner (client)
            Vector3 direction = Input.mousePosition - holder.WorldToScreenPoint(transform.position);
            OrientateBow(direction);
        }

        [ServerCallback]
        protected override void DefaultAttack()
        {
            RpcAttackAnimation();
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            LastAttackTime = Time.time;
        }

        [ServerCallback]
        protected override void SpecialAttack()
        {
            RpcAttackAnimation();
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            holder.ReduceEnergy(specialAttackCost);
            LastAttackTime = Time.time;
        }

        [ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
        private void RpcAttackAnimation() => Animator.Play("DefaultAttack");
        
        [Command] // Authority does not change the fact that sync vars must be updated on the server
        private void CmdUpdateOrientation(Vector2 bowOrientation) => orientation = bowOrientation;
    }
}