using System.Collections;
using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio.Inventories;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public class Smith : ShopKeeper {
        private const int KibryCostPerCharm = 5;

        private void Start() {
            Inventory = InventoryManager.smithInventory;
            Instantiate();
        }

        [Client]
        private IEnumerator WaitForFinalCharm(Charm finalCharm) {
            for (int i = 0; i < 10; i++) {
                if (InventoryManager.playerInventory.TryRemoveItem(finalCharm)) {
                    PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
                    GetInventory<SmithInventory>().SetResultSlot(finalCharm);
                    yield break;
                }
                
                yield return new WaitForSeconds(0.2f);
            }
        }

        [TargetRpc] private void TargetMergeSuccessful(NetworkConnection target, Charm finalCharm) {
            Inventory.ClearInventory();
            StartCoroutine(WaitForFinalCharm(finalCharm));
        }
        
        [Command(requiresAuthority = false)]
        public void CmdMergeAndAddCharm(Charm[] toMerge, Player player, NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient) return;
			
            if (!VerifyInteractionWith(player)) {
                player.TargetPrintWarning(sender, LanguageManager["#no-NPC-interaction"]);
                return;
            }
			
            if (toMerge.Length <= 1) {
                player.TargetPrintInfoMessage(sender, LanguageManager["#more-charms"]);
                return;
            }
			
            if (!player.TryReduceKibrient(KibryCostPerCharm * (toMerge.Length - 1))) {
                player.TargetPrintInfoMessage(sender, LanguageManager["#more-kibry"]);
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
        
        [Client] protected override void ClientInteract(Player player)
            => PrintDialog(new[] {"#smith-start"}, OpenInventory);
    }
}
