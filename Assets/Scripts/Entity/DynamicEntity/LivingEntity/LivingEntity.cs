using System;
using Entity.DynamicEntity.LivingEntity.PlayerClass;
using Entity.EntityInterface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class LivingEntity : DynamicEntity, IDamageable, IHealable
    {
        [SerializeField] private int totalHp;
        [NonSerialized] protected Rigidbody2D RigidBody;
        
        public int TotalHp
        {
            get => totalHp;
            set
            {
                if (value < 0)
                    return;
                totalHp = value;
                if (_currentHp > value)
                    CurrentHp = totalHp;
            }
        }

        private int _currentHp;

        public int CurrentHp
        {
            get => _currentHp;
            set
            {
                if (value > 0)
                    _currentHp = value;
                else
                {
                    _currentHp = 0;
                    OnDeath();
                }
            }
        }
        
        // Different behaviours regarding TakeDamage & GainHealth
        // for player, need to update health bar
        // We may need to implement some kind of local player
        // differentiation especially for UI, later.
        public abstract void GainHealth(int amount);
        public abstract void TakeDamage(int amount);
        protected abstract void OnDeath();

        protected void InstantiateLivingEntity()
        {
            InstantiateDynamicEntity();
            _currentHp = totalHp;
            RigidBody = GetComponent<Rigidbody2D>();
        }
    }
}
