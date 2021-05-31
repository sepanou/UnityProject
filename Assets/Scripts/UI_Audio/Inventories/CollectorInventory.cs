using System.Collections.Generic;
using Entity.Collectibles;
using Entity.DynamicEntity;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Mirror;
using TMPro;
using UnityEngine;

namespace UI_Audio.Inventories {
    public class CollectorInventory : ContainerInventory {
        private const int WeaponValue = 30;
        private const int CharmValue = 10;
        
        [SerializeField] private TMP_Text kibryDisplay;
        private int _kibryValue;
        
        private int KibryValue {
            get => _kibryValue;
            [Client] set {
                _kibryValue = value;
                kibryDisplay.text = _kibryValue.ToString();
            }
        }
        
        [Client] protected override bool CustomTryAdd(IInventoryItem item) {
            if (!(item is Weapon) && !(item is Charm) || !base.TryAddItem(item))
                return false;
            KibryValue += item is Weapon ? WeaponValue : CharmValue;
            return true;
        }

        [Client] protected override bool CustomTryRemove(IInventoryItem item) {
            if (!(item is Weapon) && !(item is Charm) || !base.TryRemoveItem(item))
                return false;
            KibryValue -= item is Weapon ? WeaponValue : CharmValue;
            return true;
        }

        public override void Close() {
            base.Close();
            KibryValue = 0;
        }

        [TargetRpc] private void TargetTransactionCompleted(NetworkConnection target) {
            ItemsMoved.Clear();
            ClearInventory();
            KibryValue = 0;
            NpcOwner.PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
        }

        [Command(requiresAuthority = false)]
        private void CmdSellItems(Weapon[] wpsToSell, Charm[] charmsToSell, Npc owner, Player player,
            NetworkConnectionToClient sender = null) {
            // Check for cheats, potential incorrect args and the possibility to proceed...
            if (sender != player.connectionToClient) return;
			
            if (!VerifyInteractionWithNpc(player, owner)) {
                player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
                return;
            }
			
            if (wpsToSell.Length == 0 && charmsToSell.Length == 0) {
                player.TargetPrintInfoMessage(sender, "There is nothing to trade...");
                return;
            }

            int kibryValue = 0;
            
            foreach (Weapon wp in wpsToSell) {
                if (!player.RemoveWeapon(wp)) continue;
                kibryValue += WeaponValue;
                NetworkServer.Destroy(wp.gameObject);
            }
            
            foreach (Charm charm in charmsToSell) {
                if (!player.RemoveCharm(charm)) continue;
                kibryValue += CharmValue;
                NetworkServer.Destroy(charm.gameObject);
            }

            if (kibryValue == 0) return;

            player.Kibrient += kibryValue;
            TargetTransactionCompleted(sender);
        }
        
        [Client] public void OnSellButtonClick() {
            List<Weapon> wps = new List<Weapon>();
            List<Charm> charms = new List<Charm>();

            foreach (IInventoryItem item in ItemsMoved) {
                if (item is Weapon wp) wps.Add(wp);
                else if (item is Charm charm) charms.Add(charm);
            }
            
            CmdSellItems(wps.ToArray(), charms.ToArray(), NpcOwner, LocalGameManager.Instance.LocalPlayer);
        }
    }
}
