using System;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class Mob : LivingEntity
    {
        [NonSerialized] protected Vector2 LastPos;

        public override void GainHealth(int amount)
        {
            CurrentHp += amount;
            Debug.Log("Health of mob increased by " + amount);
        }

        public override void TakeDamage(int amount)
        {
            Animator.Play("TakingDamage");
            Debug.Log("Health of mob decreased by " + amount);
            // Automatically calls OnDeath() if it goes under 0
            CurrentHp -= amount;
        }

        protected override void OnDeath()
        {
            Debug.Log("One bad dude sent to the Oblivion :)");
            Animator.SetBool("IsDying", true);
            gameObject.SetActive(false);
        }

        protected void InstantiateMob()
        {
            LastPos = transform.position;
            InstantiateLivingEntity();
        }
    }
}
