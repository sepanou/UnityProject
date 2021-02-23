using System;
using Entity.EntityInterface;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile
{
    public class Arrow : Projectile
    {
        protected override void Move()
        {
            RigidBody.velocity = FacingDirection * GetSpeed();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out IDamageable entity))
                entity.TakeDamage(FromWeapon.defaultDamage);
            Destroy(gameObject);
        }

        private void Update()
        {
            Move();
        }
    }
}