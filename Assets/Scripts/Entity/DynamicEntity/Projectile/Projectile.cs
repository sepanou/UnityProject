using System;
using Entity.DynamicEntity.Weapon;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile
{
    public abstract class Projectile : DynamicEntity
    {
        [NonSerialized] public RangeWeapon FromWeapon;
        [NonSerialized] public Rigidbody2D Rigidbody;
        public int damage;

        public void SpawnProjectile(RangeWeapon source, Vector2 position, Quaternion rotation)
        {
            GameObject projectile = Instantiate(gameObject, position, rotation);
            Projectile newProjectile = projectile.GetComponent<Projectile>();
            newProjectile.FromWeapon = source;
        }

        protected abstract void Move();
        public void InstantiateProjectile()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            InstantiateDynamicEntity();
        }
    }
}