using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity
{
    public abstract class DynamicEntity : Entity
    {
        /// <summary>
        /// Can represent either a velocity (= movements) or
        /// a cooldown (= weapons) depending on the context
        /// </summary>
        [SyncVar] [SerializeField] private float speed;
        protected Animator Animator;
        protected NetworkAnimator NetworkAnimator;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            writer.WriteSingle(speed);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            speed = reader.ReadSingle();
        }

        protected void InstantiateDynamicEntity()
        {
            InstantiateEntity();
            Animator = GetComponent<Animator>();
            NetworkAnimator = GetComponent<NetworkAnimator>();
        }

        public float GetSpeed() => speed;

        [ServerCallback]
        public void SetSpeed(float value)
        {
            if (value >= 0f)
                speed = value;
        }
    }
}
