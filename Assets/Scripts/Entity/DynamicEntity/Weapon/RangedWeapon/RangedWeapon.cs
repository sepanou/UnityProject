using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public abstract class RangedWeapon : Weapon
    {
        [SerializeField] protected Transform launchPoint;
        public Projectile.Projectile projectile;
        [HideInInspector] public Vector2 orientation;

        protected void InstantiateRangeWeapon()
        {
            orientation = Vector2.up;
            projectile.InstantiateProjectile();
            InitialiseWeapon();
        }
    }
}