using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
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

        [Client] protected override void ClientInteract(Player player) 
            => PrintDialog(new[] {"#innkeeper-start"}, OpenInventory);
        
        [TargetRpc] protected override void TargetItemBought(NetworkConnection target) {
            GetInventory<InnKeeperInventory>().DisplayPrice(null);
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Server] protected override void SendNotEnoughCurrencyMessage(Player player, NetworkConnectionToClient sender) 
            => player.TargetPrintInfoMessage(sender, LanguageManager["#more-kibry"]);

        [Server] protected override void ReducePlayerCurrency(Player player, int cost) 
            => player.Kibrient -= cost;

        [Server] protected override void GenerateInventory() {
            for (int i = 0; i < Inventory.Size; i++) {
                Entity generated;
                if (i < 6) {
                    if (i < 2)
                        generated = WeaponGenerator.GenerateBow(IsLootEpic);
                    else if (i < 4)
                        generated = WeaponGenerator.GenerateStaff(IsLootEpic);
                    else
                        generated = WeaponGenerator.GenerateSword(IsLootEpic);
                    
                    generated.transform.parent = transform;
                    ((Weapon) generated).SetIsGrounded(false);
                    NetworkServer.Spawn(generated.gameObject);
                }
                else {
                    generated = WeaponGenerator.GenerateCharm();
                    generated.transform.parent = transform;
                    ((Charm) generated).SetIsGrounded(false);
                    NetworkServer.Spawn(generated.gameObject);
                }

                Items.Add((IInventoryItem) generated);
            }
        }
    }
}