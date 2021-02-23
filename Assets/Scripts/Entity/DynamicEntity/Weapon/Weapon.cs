using Entity.DynamicEntity.LivingEntity.Player;
using System;
using Entity.DynamicEntity.LivingEntity;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon
{
    public abstract class Weapon : DynamicEntity
    {
        public Player holder;
        public bool equipped;
        public int defaultDamage;
        // For cooldown purposes
        protected float LastAttackTime;
        protected int specialAttackCost;
        private string _name;
        private Sprite _displayedSprite;
        protected Vector3 defaultCoordsWhenLikedToPlayer;

        public string Name { get; private set; }
        public Sprite DisplayedSprite { get; private set; }
        
        public void OnSelect<T>(T source)
        {
            if (source is Player player)
                holder = player;
            else
                return;
            transform.parent = holder.transform;
            transform.localPosition = defaultCoordsWhenLikedToPlayer;
            equipped = true;
            gameObject.SetActive(true);
        }

        public void OnUse<T>(T source)
        {
            if (!holder || !CanAttack())
                return;
            // Can't run the default & special attack simultaneously !
            if (Input.GetButtonDown("Fire1"))
                DefaultAttack();
            else if (holder.hasEnoughEnergy(specialAttackCost) && Input.GetButtonDown("Fire2"))
                SpecialAttack();
        }

        public void OnDeselect<T>(T source)
        {
            equipped = false;
            gameObject.SetActive(false);
        }

        public void OnDrop<T>(T source)
        {
            holder = null;
            equipped = false;
            gameObject.transform.parent = null;
            // source == mob or player or NPC
            if (source is LivingEntity.LivingEntity livingEntity)
                gameObject.transform.position = livingEntity.transform.position;
            gameObject.SetActive(true);
        }

        protected abstract void DefaultAttack();
        protected abstract void SpecialAttack();

        private bool CanAttack()
        {
            return holder &&
                   equipped &&
                   (float.IsNaN(LastAttackTime) || !(Time.fixedTime - LastAttackTime < GetSpeed()));
        }

        protected void InitialiseWeapon()
        {
            Name = _name;
            DisplayedSprite = _displayedSprite;
            LastAttackTime = float.NaN;
            InstantiateDynamicEntity();
        }
    }
}