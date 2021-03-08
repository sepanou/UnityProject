using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public class Bow : RangedWeapon
    {
        private bool _playerFound;
        
        private void Start()
        {
            InstantiateRangeWeapon();
            _playerFound = false;
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
        
        [ServerCallback]
        private void GroundedLogic()
        {
            if (!isGrounded || !CheckForCompatibleNearbyPlayers(out Player target)) return;
            _playerFound = true;
            StartCoroutine(Collectibles.Collectibles.OnTargetDetected(this, target));
        }

        [ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
        private void RpcAttackAnimation() => Animator.Play("DefaultAttack");

        [Command(requiresAuthority = false)]
        private void CmdOrientBow(Vector3 bowOrientation)
        {
            orientation = bowOrientation;
            orientation.Normalize();
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, orientation))
            );
        }

        private void FixedUpdate()
        {
            // Only run by server
            if (isServer && isGrounded && !_playerFound) GroundedLogic();
            if (!holder || !holder.isLocalPlayer || !equipped || isGrounded) return;
            // Only run by the weapon's owner (client)
            CmdOrientBow(Input.mousePosition - holder.WorldToScreenPoint(transform.position));
        }
    }
}