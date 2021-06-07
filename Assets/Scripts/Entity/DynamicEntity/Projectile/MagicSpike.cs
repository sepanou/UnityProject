using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
    public class MagicSpike : Projectile {
        private ParticleSystem _particleSystem;
        
        private void Start() {
            Instantiate();
            if (!TryGetComponent(out _particleSystem)) return;
            _particleSystem.Stop();
            ParticleSystem.MainModule main = _particleSystem.main;
            float rotation = -1 * transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            main.startRotationZ = new ParticleSystem.MinMaxCurve(rotation, rotation);
            _particleSystem.Play();
        }

        [Server] protected override void Move() {
            if (RigidBody.velocity != Vector2.zero) return;
            RigidBody.velocity = FacingDirection * Speed;
            // Add specific behaviour here -> Perlin noise?
        }
    }
}