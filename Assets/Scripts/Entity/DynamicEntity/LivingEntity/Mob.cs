using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class Mob : LivingEntity
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

        protected void InstantiateMob()
        {
            InstantiateLivingEntity();
        }
    }
}
