using System;
using System.Collections.Generic;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public abstract class Player : LivingEntity
    {
        private float energy;
        private Weapon.Weapon weapon;
        private List<Item.Item> inventory;
        private int money;

        public abstract void Attack(Weapon.Weapon weapon);

        public bool hasEnoughEnergy(float energy)
        {
            throw new NotImplementedException();
        }

        public void switchWeapon(Weapon.Weapon oldWeapon, Item.Item newWeapon)
        {
            throw new NotImplementedException();
        }
    }
}
