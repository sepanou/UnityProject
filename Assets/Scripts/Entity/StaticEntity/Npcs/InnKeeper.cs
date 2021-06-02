using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio.Inventories;

namespace Entity.StaticEntity.Npcs {
    public class InnKeeper : Seller {
        private void Start() {
            IsLootEpic = false;
            Inventory = InventoryManager.innKeeperInventory;
            Instantiate();
        }

        public override void OnStartServer() {
            // Necessary for generating the inventory
            Inventory = InventoryManager.innKeeperInventory;
            base.OnStartServer();
        }

        protected override int GetCost(IInventoryItem item) => item?.GetKibryValue() ?? 0;

        protected override bool HasEnoughCurrency(Player player, int cost) => player.HasEnoughKibrient(cost);

        [TargetRpc] protected override void TargetInteract(NetworkConnection target, Player player) 
            => PrintDialog(new[] {"#innkeeper-start"}, OpenInventory);
        
        [TargetRpc] protected override void TargetItemBought(NetworkConnection target) {
            GetInventory<InnKeeperInventory>().DisplayPrice(null);
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Server] protected override void SendNotEnoughCurrencyMessage(Player player, NetworkConnectionToClient sender) 
            => player.TargetPrintInfoMessage(sender, "You need more Kibrient...");

        [Server] protected override void ReducePlayerCurrency(Player player, int cost) 
            => player.Kibrient -= cost;
    }
}