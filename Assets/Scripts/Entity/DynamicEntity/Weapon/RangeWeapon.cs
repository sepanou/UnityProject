using UnityEngine;

namespace Entity.DynamicEntity.Weapon
{
    public class RangeWeapon : Weapon
    {
        [SerializeField] private Transform launchPoint;
        [SerializeField] private Projectile.Projectile projectile;

        
        protected void InstantiateRangeWeapon()
        {
            projectile.InstantiateProjectile();
        }

        protected override void Attack()
        {
            if (!CanAttack())
                return;
            // Rotates correctly the arrow to point to the FacingDirection
            Quaternion localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, holder.FacingDirection))
            );
            Animator.Play("Attack");
            projectile.SpawnProjectile(this, launchPoint.position, localRotation);
            //holder.ReducePowerSource(powerSource, unitCost);
            LastAttackTime = Time.fixedTime;
            Debug.Log("Attack ! But only " + holder.GetPowerSource(powerSource) + " stamina left...");
        }

        protected override bool CanAttack()
        {
            throw new System.NotImplementedException();
        }
    }
}