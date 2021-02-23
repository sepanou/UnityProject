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
        [SerializeField] private Vector3 defaultCoordsWhenLikedToPlayer;
    
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
            if (!holder)
                return;
        
            gameObject.transform.localRotation = Quaternion.Euler(
                new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, holder.FacingDirection))
            );
        
            if (CanAttack() && Input.GetButtonDown("Fire1"))
                Attack();
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

        protected abstract void Attack();
        protected abstract bool CanAttack();

        protected void InitialiseWeapon()
        {
            Name = _name;
            DisplayedSprite = _displayedSprite;
            LastAttackTime = float.NaN;
            InstantiateDynamicEntity();
        }
    }
}
