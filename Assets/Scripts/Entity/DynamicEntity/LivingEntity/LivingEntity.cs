using System;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity
{
    public abstract class LivingEntity : DynamicEntity
    {
        [SyncVar] private float _health;
        private bool _isAlive = true;
        protected Rigidbody2D Rigibody;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            writer.WriteSingle(_health);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            _health = reader.ReadSingle();
        }

        protected abstract void RpcDying();
        
        protected void InstantiateLivingEntity()
        {
            // Physics only simulated on the server
            // On client, no collision managed but triggers still work
            //if (TryGetComponent(out Rigibody)) Rigibody.bodyType = netIdentity.isServer
            //    ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            if (TryGetComponent(out Rigibody)) Rigibody.bodyType = RigidbodyType2D.Dynamic;
            InstantiateDynamicEntity();
        }
        
        public void TakeKnockback()
        {
            throw new NotImplementedException();
        }

        [ServerCallback]
        public void GetAttacked(int atk)
        {
            _health -= atk;
            // TakeKnockback(); Needs to be implemented
            _isAlive = _health > 0;
            RpcDying();
        }

        [ServerCallback]
        protected void ChangeHealth(float damages) => _health = Mathf.Max(_health + damages, 0);

        [ServerCallback]
        protected void SetHealth(float health) => health = Mathf.Max(health, 0);
    }
}
