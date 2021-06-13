using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public abstract class Seller : ShopKeeper {
        
        [ShowInInspector]
        protected readonly CustomSyncList<IInventoryItem> Items = new CustomSyncList<IInventoryItem>();

        protected bool IsLootEpic;

        protected abstract void TargetItemBought(NetworkConnection target);
        
        protected abstract void SendNotEnoughCurrencyMessage(Player player, NetworkConnectionToClient sender);

        protected abstract int GetCost(IInventoryItem item);

        protected abstract bool HasEnoughCurrency(Player player, int cost);

        protected abstract void ReducePlayerCurrency(Player player, int cost);

        protected abstract void GenerateInventory();

        protected new void Instantiate() {
            if (isClient) {
                Items.Callback += ItemsOnChanged;
                // SyncList callbacks are not invoked when the game object starts
                // For late joining clients notably
                foreach (IInventoryItem item in Items) {
                    Inventory.TryAddItem(item);
                    (item as Entity)?.SetSpriteRendererVisible(false);
                }
            }
            base.Instantiate();
        }

        public override void OnStartServer() {
            GenerateInventory();
            base.OnStartServer();
        }

        [Command(requiresAuthority = false)]
        public void CmdBuyItem(IInventoryItem item, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient || item is null) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, LanguageManager["#no-NPC-interaction"]);
                return;
            }

            if (item is Weapon weapon && weapon.compatibleClass != player.playerClass) {
                player.TargetPrintInfoMessage(sender, LanguageManager["#class-incompatible"]);
                return;
            }
            
            int cost = GetCost(item);
            if (!HasEnoughCurrency(player, cost)) {
                SendNotEnoughCurrencyMessage(player, sender);
                return;
            }

            if (!Items.Remove(item)) {
                player.TargetPrintWarning(sender, LanguageManager["#already-sold"]);
                return;
            }

            ReducePlayerCurrency(player, cost);
            SetSameRenderingParameters(player, (Entity) item);
            if (item is Charm charm) player.CollectCharm(charm);
            else if (item is Weapon wp) player.CollectWeapon(wp);
            else return;
            TargetItemBought(sender);
        }

        [Client] private void ItemsOnChanged(SyncList<uint>.Operation op, int index, IInventoryItem item) {
            switch (op) {
                case SyncList<uint>.Operation.OP_ADD:
                    Inventory.TryAddItem(item);
                    Entity entityItem = item as Entity;
                    if (!entityItem || entityItem is null) return;
                    entityItem.transform.SetParent(transform, false);
                    entityItem.SetSpriteRendererVisible(false);
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