using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UI_Audio.Inventories;

namespace Entity.StaticEntity.Npcs {
    public class Orchidologist : Seller {
        private void Start() {
            IsLootEpic = true;
            Inventory = InventoryManager.orchidologistInventory;
            Instantiate();
        }

        public override void OnStartServer() {
            // Necessary for generating the inventory
            Inventory = InventoryManager.orchidologistInventory;
            base.OnStartServer();
        }

        // Cost in orchids = 5 times the cost in kibrient
        protected override int GetCost(IInventoryItem item) => item?.GetKibryValue() / 5 ?? 0;

        protected override bool HasEnoughCurrency(Player player, int cost) => player.HasEnoughOrchid(cost);

        [Client] protected override void ClientInteract(Player player) 
            => PrintDialog(new[] {"#orchidologist-start"}, OpenInventory);
        
        [TargetRpc] protected override void TargetItemBought(NetworkConnection target) {
            GetInventory<OrchidologistInventory>().DisplayPrice(null);
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Server] protected override void SendNotEnoughCurrencyMessage(Player player, NetworkConnectionToClient sender) 
            => player.TargetPrintInfoMessage(sender, LanguageManager["#more-orchid"]);

        [Server] protected override void ReducePlayerCurrency(Player player, int cost) 
            => player.Orchid -= cost;

        [Server] protected override void GenerateInventory() {
            for (int i = 0; i < Inventory.Size; i++) {
                Entity generated;
                
                if (i < 2)
                    generated = WeaponGenerator.GenerateBow(IsLootEpic);
                else if (i < 4) 
                    generated = WeaponGenerator.GenerateStaff(IsLootEpic);
                else if (i < 6)
                    generated = WeaponGenerator.GenerateSword(IsLootEpic);
                else break;
                    
                generated.transform.parent = transform;
                ((Weapon) generated).SetIsGrounded(false);
                NetworkServer.Spawn(generated.gameObject);
                
                Items.Add((IInventoryItem) generated);
            }
        }
    }
}