using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UI_Audio.Inventories;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public abstract class ShopKeeper : Npc
    {
        protected NpcInventory Inventory;

        protected T GetInventory<T>() where T : NpcInventory => (T) Inventory;
        
        protected new void Instantiate() {
            base.Instantiate();
            Inventory.ClearInventory();
            Inventory.SetNpcOwner(this);
            Inventory.Close();
        }
        
        public override void OnStartClient() {
            base.OnStartClient();
            CmdSynchronizePosition();
        }

        [Client] protected void OpenInventory() {
            Player localPlayer = LocalGameManager.Instance.LocalPlayer;
            if (!VerifyInteractionWith(localPlayer))
                return;
			
            if (Inventory is ContainerInventory container) {
                InventoryManager.OpenSidedInventory();
                container.Open();
                localPlayer.SetContainerInventory(container);
            } else if (Inventory is SellerInventory seller) {
                InventoryManager.OpenMiddleInventory();
                seller.Open();
                localPlayer.SetSellerInventory(seller);
            }
        }
        
        [ClientCallback] protected override void OnTriggerExit2D(Collider2D other) {
            if (other.TryGetComponent(out Player player) && player.isLocalPlayer)
                InventoryManager.CloseAllInventories();
            base.OnTriggerExit2D(other);
        }
    }
}
