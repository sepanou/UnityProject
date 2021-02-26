using UnityEngine;

namespace Entity.DynamicEntity.Projectile
{
    public class Arrow : Projectile
    {
        private void Start()
        {
            InstantiateProjectile();
        }
        protected override void Move()
        {
            RigidBody.velocity = FacingDirection * GetSpeed();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out LivingEntity.LivingEntity entity))
                entity.GetAttacked(FromWeapon.defaultDamage);
            Destroy(gameObject);
        }

        private void Update()
        {
            Move();
        }
    }
}