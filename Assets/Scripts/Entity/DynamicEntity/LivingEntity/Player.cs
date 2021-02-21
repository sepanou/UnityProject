using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class Player : LivingEntity
    {
        public override void GainHealth(int amount)
        {
            CurrentHp += amount;
            Debug.Log("Health increased by " + amount);
            // Add behaviour later for UI (modify HealthBar)
        }

        public override void TakeDamage(int amount)
        {
            CurrentHp -= amount;
            Debug.Log("Health decreased by " + amount);
            // Add behaviour later for UI (modify HealthBar)
        }

        protected override void OnDeath()
        {
            Debug.Log("Too bad you're dead :(");
            gameObject.SetActive(false);
        }

        protected void Move()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);
            direction.Normalize();
            RigidBody.velocity = GetSpeed() * direction;
        }

        protected void InstantiatePlayer()
        {
            InstantiateLivingEntity();
        }
    }
}
