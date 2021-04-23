using System;
using System.Collections;
using System.Collections.Generic;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity
{
    
    [RequireComponent(typeof(Collider2D))]
    public class NPC : DynamicEntity
    {
        private enum NPC_Type
        {
            Smith,
            Seller,
            Buyer,
            ClassSelector,
            StoryTeller
        }
        
        // A NPC is interactive !
        private Rigidbody2D _rigidBody; // For movements
        private Collider2D _collider;
        // Contains all the players in the interaction area
        // (Player, bool -> has he interacted already)
        private HashSet<Player> _playerPool;
        
        [SerializeField] private PlayerClasses classType;
        [SerializeField] private NPC_Type npcType;

        private void Start()
        {
            TryGetComponent(out _rigidBody);
            TryGetComponent(out _collider);
            _collider.isTrigger = true;
            _playerPool = new HashSet<Player>();
        }

        private IEnumerator CheckInteraction(Player player)
        {
            yield break;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out Player player))
                return;
            if (isServer)
                _playerPool.Add(player);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out Player player))
                return;
            if (isServer)
                _playerPool.Remove(player);
        }

        [ServerCallback]
        public bool CanPlayerInteractWithNPC(Player player) => _playerPool.Contains(player);

        public void Interact(Player player)
        {
            
        }
    }
}