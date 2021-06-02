using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio.Inventories;

namespace Entity.StaticEntity.Npcs {
    public class Smith : ShopKeeper {
        private const int KibryCostPerCharm = 5;

        private void Start() {
            Inventory = InventoryManager.smithInventory;
            Instantiate();
        }

        [TargetRpc] private void TargetMergeSuccessful(NetworkConnection target, Charm finalCharm) {
            Inventory.ClearInventory();
			
            if (!InventoryManager.playerInventory.TryRemoveItem(finalCharm))
                return; // Should not happen
			
            PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
            GetInventory<SmithInventory>().SetResultSlot(finalCharm);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdMergeAndAddCharm(Charm[] toMerge, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
                return;
            }
			
            if (toMerge.Length <= 1) {
                player.TargetPrintInfoMessage(sender, "Not enough charms to merge - add more");
                return;
            }
			
            if (!player.TryReduceKibrient(KibryCostPerCharm * (toMerge.Length - 1))) {
                player.TargetPrintInfoMessage(sender, "You need more Kibrient...");
                return;
            }
			
            CharmData result = null;
            foreach (Charm charm in toMerge) {
                if (!player.RemoveCharm(charm)) continue;
                result += charm.bonuses;
                NetworkServer.Destroy(charm.gameObject);
            }

            if (result is null) return;
			
            Charm finalCharm = LocalGameManager.Instance.weaponGenerator.GenerateCharm(result);
            NetworkServer.Spawn(finalCharm.gameObject);
            player.CollectCharm(finalCharm);

            TargetMergeSuccessful(sender, finalCharm);
        }
        
        [TargetRpc] protected override void TargetInteract(NetworkConnection target, Player player) 
            => PrintDialog(new[] {"#smith-start"}, OpenInventory);
    }
}
