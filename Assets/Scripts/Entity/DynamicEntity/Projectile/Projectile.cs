using System;
using Entity.DynamicEntity.Weapon;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile
{
    public abstract class Projectile : DynamicEntity
    {
        [NonSerialized] protected RangeWeapon FromWeapon;
        [NonSerialized] protected Rigidbody2D RigidBody;
        [NonSerialized] protected Vector2 FacingDirection;

        public static void SpawnProjectile(RangeWeapon source, Vector2 position)
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