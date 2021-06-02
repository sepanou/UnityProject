using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;

namespace Entity.StaticEntity.Npcs {
    public class InnKeeper : Seller {
        private void Start() {
            IsLootEpic = false;
            Inventory = InventoryManager.innKeeperInventory;
            Instantiate();
        }

        protected override int GetCost(IInventoryItem item) => item?.GetKibryValue() ?? 0;

        [TargetRpc] protected override void TargetInteract(NetworkConnection target, Player player) 
            => PrintDialog(new[] {"#innkeeper-start"}, OpenInventory);
        
        [TargetRpc] protected override void TargetItemBought(NetworkConnection target) {
            //kibryDisplay.text = "0";
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Server] protected override bool TryReducePlayerCurrency(Player player, int cost, NetworkConnectionToClient sender) {
            if (player.TryReduceKibrient(cost)) return true;
            player.TargetPrintInfoMessage(sender, "You need more Kibrient...");
            return false;
        }
    }
}