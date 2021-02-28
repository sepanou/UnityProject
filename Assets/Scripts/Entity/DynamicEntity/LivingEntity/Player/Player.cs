using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public abstract class Player : LivingEntity
    {
        [SerializeField] private int energy;
        [SerializeField] protected Weapon.Weapon weapon;
        private List<Item.Item> _inventory;
        private int _money = 0;

        public bool HasEnoughEnergy(int amount) => energy >= amount;

        public void ReduceEnergy(int amount)
        {
            energy = amount >= energy ? 0 : energy - amount;
        }

        protected void Move()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);
            direction.Normalize();
            Rigibody.velocity = GetSpeed() * direction;
        }

        public void SwitchWeapon(Weapon.Weapon oldWeapon, Item.Item newWeapon)
        {
            throw new NotImplementedException();
        }

        protected void InstantiatePlayer()
        {
            InstantiateLivingEntity();
        }
    }
}
