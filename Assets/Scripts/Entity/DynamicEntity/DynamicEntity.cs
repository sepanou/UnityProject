using System;
using UnityEngine;

namespace Entity.DynamicEntity
{
    public abstract class DynamicEntity : Entity
    {
        [SerializeField] private float speed;
        [NonSerialized] public Animator Animator;

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
