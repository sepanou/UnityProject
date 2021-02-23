using System;
using Entity.DynamicEntity.LivingEntity;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangeWeapons
{
    public class Bow : RangeWeapon
    {
        private void Start()
        {
            InstantiateRangeWeapon();
        }

        protected override void DefaultAttack()
        {
            Animator.Play("DefaultAttack");
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            LastAttackTime = Time.fixedTime;
        }
        
        protected override void SpecialAttack()
        {
            Animator.Play("SpecialAttack");
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            holder.ReducePower(specialAttackCost);
            LastAttackTime = Time.fixedTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (holder || !Input.GetKeyDown(KeyCode.E) || !other.gameObject.TryGetComponent(out Player player))
                return;
            if (player.EquippedWeapon)
                player.EquippedWeapon.OnDrop(player);
            player.EquippedWeapon = this;
            holder = player;
            equipped = true;
            gameObject.transform.parent = player.transform;
            gameObject.transform.localPosition = defaultCoordsWhenLikedToPlayer;
        }

        private void Update()
        {
            // Orients correctly the bow
            if (!holder || holder.FacingDirection.magnitude < 0.1f)
                return;
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, holder.FacingDirection))
            );
            Orientation = holder.FacingDirection;
        }
    }
}