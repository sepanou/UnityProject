using Entity.DynamicEntity.Weapon.RangedWeapon;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile
{
    public abstract class Projectile : DynamicEntity
    {
        protected RangedWeapon FromWeapon;
        protected Rigidbody2D RigidBody;
        protected Vector2 FacingDirection;

        public static void SpawnProjectile(RangedWeapon source, Vector2 position)
        {
            Quaternion localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, source.Orientation))
            );
            GameObject projectile = Instantiate(source.projectile.gameObject, position, localRotation);
            Projectile newProjectile = projectile.GetComponent<Projectile>();
            newProjectile.FromWeapon = source;
            newProjectile.FacingDirection = source.Orientation;
            newProjectile.InstantiateProjectile();
        }

        protected abstract void Move();

        public void InstantiateProjectile()
        {
            RigidBody = GetComponent<Rigidbody2D>();
            InstantiateDynamicEntity();
        }
    }
}
