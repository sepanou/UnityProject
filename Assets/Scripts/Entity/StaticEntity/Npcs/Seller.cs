using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public abstract class Seller : ShopKeeper {
        
        [ShowInInspector]
        private readonly CustomSyncList<IInventoryItem> _items = new CustomSyncList<IInventoryItem>();

        protected bool IsLootEpic;

        protected abstract void TargetItemBought(NetworkConnection target);

        protected abstract int GetCost(IInventoryItem item);

        protected abstract bool TryReducePlayerCurrency(Player player, int cost, NetworkConnectionToClient sender);
        
        protected new void Instantiate() {
            _items.SetCoroutineHandler(this);
            if (isLocalPlayer) {
                _items.Callback += ItemsOnChanged;
                // SyncList callbacks are not invoked when the game object starts
                foreach (IInventoryItem item in _items)
                    Inventory.TryAddItem(item);
            }
            base.Instantiate();
        }

        [Server] private void GenerateInventory() {
            for (int i = 0; i < Inventory.Size; i++) {
                Entity generated;
                int rdm = Random.Range(0, 4);

                if (rdm == 0)
                    generated = WeaponGenerator.GenerateCharm();
                else if (rdm == 1)
                    generated = WeaponGenerator.GenerateBow(IsLootEpic);
                else if (rdm == 2)
                    generated = WeaponGenerator.GenerateStaff(IsLootEpic);
                else
                    generated = WeaponGenerator.GenerateSword(IsLootEpic);

                generated.DisableInteraction(null);
                generated.transform.parent = transform;
                NetworkServer.Spawn(generated.gameObject);
                
                if (generated is Charm charm) {
                    charm.SetIsGrounded(false);
                    _items.Add(charm);
                }
                else if (generated is Weapon wp) {
                    wp.SetIsGrounded(false);
                    _items.Add(wp);
                }
            }
            
            Inventory.ClearInventory();
            foreach (IInventoryItem item in _items)
                Inventory.TryAddItem(item);
        }

        [Command(requiresAuthority = false)]
        public void CmdBuyItem(IInventoryItem item, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient || item is null) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
                return;
            }

            if (!_items.Remove(item)) {
                player.TargetPrintWarning(sender, "Too late...\nThis item has already been sold!");
                return;
            }
            
            if (item is Charm charm) player.AddCharm(charm);
            else if (item is Weapon wp) player.AddWeapon(wp);
            else return;
            
            int cost = GetCost(item);
            
            if (!TryReducePlayerCurrency(player, cost, sender)) return;

            TargetItemBought(sender);
        }

        [Client] private void ItemsOnChanged(SyncList<uint>.Operation op, int index, IInventoryItem item) {
            switch (op) {
                case SyncList<uint>.Operation.OP_ADD:
                    Inventory.TryAddItem(item);
                    break;
                case SyncList<uint>.Operation.OP_CLEAR:
                    Inventory.ClearInventory();
                    break;
                case SyncList<uint>.Operation.OP_REMOVEAT:
                    Inventory.TryRemoveItem(item);
                    break;
                default:
                    Debug.LogWarning("[SyncList] Operation not handled.");
                    break;
            }
        }
        
        [ServerCallback] protected override void OnTriggerEnter2D(Collider2D other) {
            // Generates the inventory when everything has been bought
            if (_items.Count == 0)
                GenerateInventory();
            base.OnTriggerEnter2D(other);
        }
    }
}