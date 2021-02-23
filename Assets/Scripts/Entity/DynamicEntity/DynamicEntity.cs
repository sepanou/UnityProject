using System;
using UnityEngine;

namespace Entity.DynamicEntity
{
    public abstract class DynamicEntity : Entity
    {
        /// <summary>
        /// Can represent either a velocity (= movements) or
        /// a cooldown (= weapons) depending on the context
        /// </summary>
        [SerializeField] private float speed;
        [NonSerialized] protected Animator Animator;

        public float GetSpeed()
        {
            return speed;
        }

        public void SetSpeed(float value)
        {
            if (value >= 0f)
                speed = value;
        }

        protected void InstantiateDynamicEntity()
        {
            InstantiateEntity();
            Animator = GetComponent<Animator>();
        }
    }
}
