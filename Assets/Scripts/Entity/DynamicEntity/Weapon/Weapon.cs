using Entity.DynamicEntity.LivingEntity.Player;
using System.Collections.Generic;
using DataBanks;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon
{
    public abstract class Weapon : DynamicEntity
    {
        public Player holder;
        private string _name;
        private ContactFilter2D _filter;
        
        [SyncVar] public PlayerClasses compatibleClass;
        [SyncVar] public bool equipped, isGrounded;
        [SyncVar] protected float LastAttackTime; // For cooldown purposes
        [SyncVar] private GameObject _holder; // Only for sync (for new players)

        [SerializeField] protected int defaultDamage;
        [SerializeField] protected int specialAttackCost;
        [SerializeField] protected WeaponGeneratorDB weaponGenerator;
        [SerializeField] protected Vector3 defaultCoordsWhenLikedToPlayer;

        protected void InstantiateWeapon()
        {
            isGrounded = true; // By default, weapon on the ground !
            LastAttackTime = float.NaN;
            _filter = new ContactFilter2D();
            InstantiateDynamicEntity();
        }
        
        public int GetDamage() => defaultDamage;
        public int GetSpecialAttackCost() => specialAttackCost;
        
        public bool CanAttack()
        {
            return holder && equipped &&
                   (float.IsNaN(LastAttackTime) || !(Time.time - LastAttackTime < GetSpeed()));
        }
        
        protected abstract void DefaultAttack();
        protected abstract void SpecialAttack();

        [ServerCallback]
        protected bool CheckForCompatibleNearbyPlayers(out Player compatiblePlayer)
        {
            List<Collider2D> results = new List<Collider2D>();
            Physics2D.OverlapCircle(transform.position, 2f, _filter.NoFilter(), results);
            
            foreach (Collider2D obj in results)
            {
                if (!obj.gameObject.TryGetComponent(out Player player) ||
                    player.playerClass != compatibleClass) continue;
                compatiblePlayer = player;
                return true;
            }

            compatiblePlayer = null;
            return false;
        }
        
        // Validation checks before attacking
        // Only the player with authority on the object can call this method
        [ServerCallback]
        public void UseWeapon(bool fireOneButton, bool fireTwoButton)
        {
            if (fireOneButton) DefaultAttack();
            else if (fireTwoButton && holder.HasEnoughEnergy(specialAttackCost)) SpecialAttack();
        }

        [ClientRpc]
        public void RpcSetWeaponParent(Transform parent) => transform.parent = parent;

        [ClientRpc]
        public void RpcUnEquip()
        {
            equipped = false;
            gameObject.SetActive(false);
        }

        [ClientRpc]
        public void RpcEquip(Player source)
        {
            _holder = source.gameObject;
            holder = source;
            transform.localPosition = defaultCoordsWhenLikedToPlayer;
            equipped = true;
            gameObject.SetActive(true);
        }
        
        // Called on client every time this object is spawned (especially when new players join)
        [ClientCallback]
        public override void OnStartClient()
        {
            if (!_holder || !_holder.TryGetComponent(out Player player)) return;
            var transfo = transform;
            transfo.parent = player.transform;
            transfo.position = defaultCoordsWhenLikedToPlayer;
        }
    }
}