using System;
using System.Collections.Generic;
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
        
        // Used for name generation
        
        public Dictionary<int, (int, string)> WeaponName=
            new Dictionary<int,(int, string)>()
            {
                {0, (0, "L'arc")},                // 0 == masculine adjective, feminine otherwise.
                {1, (0, "L'arc court")},
                {2, (0, "L'arc long")},
                {3, (0, "L'arc monobloc")},
                {4, (0, "L'arc à poulies")},
                {5, (0, "L'arc droit")},
                {6, (0, "L'arc de chasse")},
                {7, (0, "L'arc Yumi")}
            };
    }
}