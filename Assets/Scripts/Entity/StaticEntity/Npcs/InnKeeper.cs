using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UI_Audio.Inventories;
using UnityEngine;

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
                    
                    generated.DisableInteraction(null);
                    generated.transform.parent = transform;
                    NetworkServer.Spawn(generated.gameObject);
                    ((Weapon) generated).SetIsGrounded(false);
                }
                else {
                    generated = WeaponGenerator.GenerateCharm();
                    generated.DisableInteraction(null);
                    generated.transform.parent = transform;
                    NetworkServer.Spawn(generated.gameObject);
                    ((Charm) generated).SetIsGrounded(false);
                }

                Items.Add((IInventoryItem) generated);
            }
        }
    }
}