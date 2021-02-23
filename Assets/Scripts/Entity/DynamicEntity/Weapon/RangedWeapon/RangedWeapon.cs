using System;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public abstract class RangedWeapon : Weapon
    {
        protected Transform launchPoint;
        public global::Entity.DynamicEntity.Projectile.Projectile projectile;
        public Vector2 Orientation;

        protected void InstantiateRangeWeapon()
        {
            Orientation = Vector2.up;
            projectile.InstantiateProjectile();
            InitialiseWeapon();
        }
    }
}