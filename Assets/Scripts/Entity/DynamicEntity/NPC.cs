﻿using System;
using System.Collections;
using System.Collections.Generic;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio;
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
        // (Player, bool -> has he currently interacting)
        private Dictionary<Player, bool> _playerPool; // Server only
        private bool _canInteract, _isInteracting; // Client only

        [SerializeField] private PlayerClasses classType;
        [SerializeField] private NPC_Type npcType;

        [NonSerialized] public static PlayerInfoManager InfoManager;
        [NonSerialized] public static InventoryManager InventoryManager;
        [NonSerialized] public static LanguageManager LanguageManager;
        [NonSerialized] public static InputManager InputManager;
        

        private void Start()
        {
            TryGetComponent(out _rigidBody);
            TryGetComponent(out _collider);
            _collider.isTrigger = true;
            _playerPool = new Dictionary<Player, bool>();
            _canInteract = false;
        }

        [ClientCallback]
        private IEnumerator CheckInteraction(Player player)
        {
            InfoManager.SetInfoText(LanguageManager["interact"]);
            InfoManager.OpenInfoBox();
            
            while (_canInteract)
            {
                while (!InputManager.GetKeyDown("Interact"))
                {
                    if (!_canInteract)
                    {
                        InfoManager.CloseInfoBox();
                        yield break;
                    }

                    yield return null;
                }

                if (!_isInteracting)
                    CmdInteract(player);
                
                yield return null;
            }
            
            InfoManager.CloseInfoBox();
        }

        [ClientCallback]
        private void StopInteracting() => _isInteracting = false;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out Player player))
                return;
            
            if (isServer)
                _playerPool[player] = false;

            if (!player.isLocalPlayer) return;
            
            _canInteract = true;
            StopAllCoroutines();
            StartCoroutine(CheckInteraction(player));
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out Player player))
                return;
            
            if (isServer)
                _playerPool.Remove(player);

            if (player.isLocalPlayer)
                _canInteract = false;
        }

        [ServerCallback]
        private bool CanPlayerInteractWithNPC(Player player) => _playerPool.ContainsKey(player);

        [Command(requiresAuthority = false)]
        private void CmdInteract(Player player)
        {
            if (!CanPlayerInteractWithNPC(player))
                return;

            switch (npcType)
            {
                case NPC_Type.Smith:
                    InteractSmith(player.connectionToClient, player);
                    break;
                case NPC_Type.Seller:
                    InteractSeller(player.connectionToClient, player);
                    break;
                case NPC_Type.Buyer:
                    InteractBuyer(player.connectionToClient, player);
                    break;
                case NPC_Type.ClassSelector:
                    InteractClassSelector(player.connectionToClient, player);
                    break;
                case NPC_Type.StoryTeller:
                    InteractStoryTeller(player.connectionToClient, player);
                    break;
            }
        }

        [TargetRpc]
        private void InteractSmith(NetworkConnection target, Player player)
        {
            InventoryManager.smithInventory.Open();
            // TODO
        }
        
        [TargetRpc]
        private void InteractBuyer(NetworkConnection target, Player player)
        {
            // TODO
        }
        
        [TargetRpc]
        private void InteractSeller(NetworkConnection target, Player player)
        {
            // TODO
        }
        
        [TargetRpc]
        private void InteractStoryTeller(NetworkConnection target, Player player)
        {
            // TODO
        }
        
        [TargetRpc]
        private void InteractClassSelector(NetworkConnection target, Player player)
        {
            _isInteracting = true;
            switch (classType)
            {
                case PlayerClasses.Archer:
                    InfoManager.PrintDialog(new [] {"#archer-selector"}, StopInteracting);
                    player.CmdSwitchPlayerClass(PlayerClasses.Archer);
                    break;
                case PlayerClasses.Mage:
                    InfoManager.PrintDialog(new [] {"#mage-selector"}, StopInteracting);
                    player.CmdSwitchPlayerClass(PlayerClasses.Mage);
                    break;
                default:
                    InfoManager.PrintDialog(new [] {"#warrior-selector"}, StopInteracting);
                    player.CmdSwitchPlayerClass(PlayerClasses.Warrior);
                    break;
            }
        }
    }
}