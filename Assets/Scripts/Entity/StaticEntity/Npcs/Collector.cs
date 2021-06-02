using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UI_Audio.Inventories;

namespace Entity.StaticEntity.Npcs {
    public class Collector : ShopKeeper
    {
        private void Start() {
            Inventory = InventoryManager.collectorInventory;
            Instantiate();
        }
        
        [TargetRpc] private void TargetTransactionCompleted(NetworkConnection target) {
            Inventory.ClearInventory();
            GetInventory<CollectorInventory>().ResetValue();
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Command(requiresAuthority = false)]
        public void CmdSellItems(IInventoryItem[] itemsToSell, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
                return;
            }
			
            if (itemsToSell.Length == 0) {
                player.TargetPrintInfoMessage(sender, "There is nothing to trade...");
                return;
            }

            int kibryValue = 0;

            foreach (IInventoryItem item in itemsToSell) {
                if (item is Weapon wp && !player.RemoveWeapon(wp))
                    continue;
                if (item is Charm charm && !player.RemoveCharm(charm))
                    continue;
                kibryValue += item.GetKibryValue();
                NetworkServer.Destroy(item.GetGameObject());
            }

            if (kibryValue == 0) return;

            player.Kibrient += kibryValue / 2; // Half price
            TargetTransactionCompleted(sender);
        }
        
        [TargetRpc] protected override void TargetInteract(NetworkConnection target, Player player) 
            => PrintDialog(new[] {"#collector-start"}, OpenInventory);
    }
}
