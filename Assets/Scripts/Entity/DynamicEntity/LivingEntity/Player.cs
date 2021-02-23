using System;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public enum PowerTypes
    {
        Stamina,
        Mana,
        Unlimited
    }
    
    public abstract class Player : LivingEntity
    {
        // Useful for orientating the bow or launching points :)
        [NonSerialized] public Vector2 FacingDirection;
        // Don't know if there's a need to differentiate 3 C# classes for each in-game classes
        // Just use this parameter to determine the class
        [NonSerialized] public PowerTypes PowerType;
        [SerializeField] private int powerValue;
        [NonSerialized] public Weapon.Weapon EquippedWeapon;

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
            // gameObject.SetActive(false);
        }

        protected void Move()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);
            direction.Normalize();
            RigidBody.velocity = GetSpeed() * direction;
            FacingDirection = direction;
        }

        public bool HasEnoughPower(int amountNeeded) => amountNeeded <= powerValue;

        public void ReducePower(int amount)
        {
            powerValue = amount >= powerValue ? 0 : powerValue - amount;
            Debug.Log("Power reduced to " + powerValue);
        }

        private void Update()
        {
            Move();
            if (!EquippedWeapon)
                return;
            EquippedWeapon.OnUse(this);
        }
        

        protected void InstantiatePlayer()
        {
            FacingDirection = Vector2.up;
            InstantiateLivingEntity();
        }
    }
}
