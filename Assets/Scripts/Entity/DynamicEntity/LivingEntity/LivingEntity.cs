using System;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class LivingEntity : DynamicEntity
    {
        private float _health;
        private bool _isAlive = true;
        protected Rigidbody2D Rigibody;

        public void GetAttacked(int atk)
        {
            _health -= atk;
            // TakeKnockback(); Needs to be implemented
            _isAlive = _health > 0;
            Dying();
        }

        public void TakeKnockback()
        {
            throw new NotImplementedException();
        }

        protected abstract void Dying();

        protected abstract void Attack();

        protected abstract void GetAttacked();

        protected void ChangeHealth(float damages) // Negative damages == healing
        {
            _health += damages;
        }

        protected void SetHealth(float health)
        {
            _health = health;
        }

        protected void InstantiateLivingEntity()
        {
            InstantiateDynamicEntity();
            Rigibody = GetComponent<Rigidbody2D>();
        }
    }
}
