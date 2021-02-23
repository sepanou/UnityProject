using System;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon
{
    public abstract class RangeWeapon : Weapon
    {
        [SerializeField] protected Transform launchPoint;
        [SerializeField] public Projectile.Projectile projectile;
        [NonSerialized] public Vector2 Orientation;

        protected void InstantiateRangeWeapon()
        {
            Orientation = Vector2.up;
            projectile.InstantiateProjectile();
            InitialiseWeapon();
        }
    }
}