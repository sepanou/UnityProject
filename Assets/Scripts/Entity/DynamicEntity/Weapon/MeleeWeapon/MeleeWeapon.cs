using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.MeleeWeapon
{
    public class MeleeWeapon : Weapon
    {
        private void OrientateBow(Vector3 bowOrientation)
        {
            if (!hasAuthority) return;
            bowOrientation.Normalize();
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, bowOrientation)));
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
            LastAttackTime = Time.time;
        }

        [ServerCallback]
        protected override void SpecialAttack()
        {
            RpcAttackAnimation();
            holder.ReduceEnergy(specialAttackCost);
            LastAttackTime = Time.time;
        }

        [ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
        private void RpcAttackAnimation() => Animator.Play("DefaultAttack");
    }
}
