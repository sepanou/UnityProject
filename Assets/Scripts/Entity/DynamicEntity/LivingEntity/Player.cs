using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class Player : LivingEntity
    {
        public override void GainHealth(int amount)
        {
            throw new System.NotImplementedException();
        }

        public override void TakeDamage(int amount)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDeath()
        {
            throw new System.NotImplementedException();
        }

        protected void Move()
        {
            // TODO -> makes the player move based on inputs
        }

        protected void InstantiatePlayer()
        {
            InstantiateLivingEntity();
        }
    }
}
