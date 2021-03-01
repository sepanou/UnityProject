using System;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public class Bow : RangedWeapon
    {
        private void Start()
        {
            InitialiseWeapon();
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
            holder.ReduceEnergy(specialAttackCost);
            LastAttackTime = Time.fixedTime;
        }
        
        private void Update()
        {
            // Orients correctly the bow
            orientation = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            orientation.Normalize();
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, orientation))
            );
        }
    }
}