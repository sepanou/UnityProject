using System;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public abstract class Player : LivingEntity
    {
        private float energy;
        private Weapon.Weapon weapon;
        private Item.Item[] inventory;

        public abstract void Attack(Weapon.Weapon weapon);

        public bool hasEnoughEnergy(float energy)
        {
            throw new NotImplementedException();
        }
    }
}
