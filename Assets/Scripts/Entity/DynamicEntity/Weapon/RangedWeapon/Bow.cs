using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public class Bow : RangedWeapon
    {
        [SyncVar] private bool _playerFound;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            writer.WriteBoolean(_playerFound);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            _playerFound = reader.ReadBoolean();
        }

        private void Start()
        {
            InstantiateRangeWeapon();
            if (isServer)
                _playerFound = false;
        }
        
        private void FixedUpdate()
        {
            // Only run by server
            if (isServer && isGrounded && !_playerFound) GroundedLogic();
            if (!holder || !holder.isLocalPlayer || !equipped || isGrounded) return;
            // Only run by the weapon's owner (client)
            Vector3 direction = Input.mousePosition - holder.WorldToScreenPoint(transform.position);
            OrientBow(direction);
            CmdOrientBow(direction);
        }

        private void OrientBow(Vector2 orient) => gameObject.transform.localRotation = 
            Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, orient)));

        [ServerCallback]
        protected override void DefaultAttack()
        {
            RpcAttackAnimation();
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            LastAttackTime = Time.time;
        }

        [ServerCallback]
        protected override void SpecialAttack()
        {
            RpcAttackAnimation();
            Projectile.Projectile.SpawnProjectile(this, launchPoint.position);
            holder.ReduceEnergy(specialAttackCost);
            LastAttackTime = Time.time;
        }
        
        [ServerCallback]
        private void GroundedLogic()
        {
            if (!isGrounded || !CheckForCompatibleNearbyPlayers(out Player target)) return;
            _playerFound = true;
            StartCoroutine(Collectibles.Collectibles.OnTargetDetected(this, target));
        }

        [ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
        private void RpcAttackAnimation() => Animator.Play("DefaultAttack");

        [Command(requiresAuthority = false)]
        private void CmdOrientBow(Vector3 bowOrientation)
        {
            orientation = bowOrientation;
            orientation.Normalize();
            OrientBow(orientation);
        }
    }
}