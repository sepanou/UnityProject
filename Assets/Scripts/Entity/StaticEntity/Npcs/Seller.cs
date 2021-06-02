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
        
        protected abstract void SendNotEnoughCurrencyMessage(Player player, NetworkConnectionToClient sender);

        protected abstract int GetCost(IInventoryItem item);

        protected abstract bool HasEnoughCurrency(Player player, int cost);

        protected abstract void ReducePlayerCurrency(Player player, int cost);

        protected new void Instantiate() {
            if (isClient) {
                _items.Callback += ItemsOnChanged;
                // SyncList callbacks are not invoked when the game object starts
                // For late joining clients notably
                foreach (IInventoryItem item in _items)
                    Inventory.TryAddItem(item);
            }
            base.Instantiate();
        }

        public override void OnStartServer() {
            GenerateInventory();
            base.OnStartServer();
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
        }

        [Command(requiresAuthority = false)]
        public void CmdBuyItem(IInventoryItem item, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient || item is null) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
                return;
            }

            int cost = GetCost(item);
            if (!HasEnoughCurrency(player, cost)) {
                SendNotEnoughCurrencyMessage(player, sender);
                return;
            }

            if (!_items.Remove(item)) {
                player.TargetPrintWarning(sender, "Too late...\nThis item has already been sold!");
                return;
            }

            ReducePlayerCurrency(player, cost);
            if (item is Charm charm) player.CollectCharm(charm);
            else if (item is Weapon wp) player.CollectWeapon(wp);
            else return;
            TargetItemBought(sender);
        }

        [Client] private void ItemsOnChanged(SyncList<uint>.Operation op, int index, IInventoryItem item) {
            switch (op) {
                case SyncList<uint>.Operation.OP_ADD:
                    (item as Entity)?.transform.SetParent(transform, false);
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
    }
}