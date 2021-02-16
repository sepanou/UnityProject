using System;
using Entity.DynamicEntity.LivingEntity;
using Entity.EntityInterface;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon
{
    public abstract class Weapon : DynamicEntity, IDisplayableItem
    {
        public Player holder;
        public bool equipped;
        // For cooldown purposes
        [NonSerialized] protected float LastAttackTime;
        [SerializeField] private string _name;
        [SerializeField] private Sprite _displayedSprite;
    
        public string Name { get; private set; }
        public Sprite DisplayedSprite { get; private set; }
        
        public void OnSelect<T>(T source)
        {
            throw new NotImplementedException();
        }

        public void OnUse<T>(T source)
        {
            throw new NotImplementedException();
        }

        public void OnDeselect<T>(T source)
        {
            throw new NotImplementedException();
        }

        public void OnDrop<T>(T source)
        {
            throw new NotImplementedException();
        }

        protected abstract void Attack();
        protected abstract void CanAttack();

        protected void InitialiseWeapon()
        {
            Name = _name;
            DisplayedSprite = _displayedSprite;
            LastAttackTime = float.NaN;
            InstantiateDynamicEntity();
        }
    }
}
