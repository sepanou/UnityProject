using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class Mob : LivingEntity
    {
        public override void GainHealth(int amount)
        {
            CurrentHp += amount;
            Debug.Log("Health increased by " + amount);
        }

        public override void TakeDamage(int amount)
        {
            CurrentHp -= amount;
            Debug.Log("Health decreased by " + amount);
        }

        protected override void OnDeath()
        {
            Debug.Log("One bad dude sent to the Oblivion :)");
            gameObject.SetActive(false);
        }

        protected void InstantiateMob()
        {
            InstantiateLivingEntity();
        }
    }
}
