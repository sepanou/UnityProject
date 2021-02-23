using UnityEngine;

namespace Entity.DynamicEntity
{
    public abstract class DynamicEntity : Entity
    {
        /// <summary>
        /// Can represent either a velocity (= movements) or
        /// a cooldown (= weapons) depending on the context
        /// </summary>
        private float _speed;
        protected Animator Animator;
        protected Collider2D Collider;

        public float GetSpeed()
        {
            return _speed;
        }

        public void SetSpeed(float value)
        {
            if (value >= 0f)
                _speed = value;
        }

        protected void InstantiateDynamicEntity()
        {
            InstantiateEntity();
            Animator = GetComponent<Animator>();
            Collider = GetComponent<Collider2D>();
        }
    }
}
