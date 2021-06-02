using System.Linq;
using Entity.Collectibles;
using Entity.StaticEntity.Npcs;
using UnityEngine;

namespace UI_Audio.Inventories {
	public class SmithInventory: ContainerInventory {
		[SerializeField] private InventorySlot resultSlot;

		protected override bool CustomTryAdd(IInventoryItem item) => item is Charm && base.TryAddItem(item);
		
		protected override bool CustomTryRemove(IInventoryItem item) => item is Charm && base.TryRemoveItem(item);

		private void Start() => Dialog = new[] {"#smith-stop"};
		
		public override void Close() { 
			base.Close();
			resultSlot.ClearItem();
		}

		public override void TryMoveHoveredSlotItem(Inventory playerInventory) {
			if (!InventorySlot.LastHovered) return;
			
			if (InventorySlot.LastHovered != resultSlot) {
				base.TryMoveHoveredSlotItem(playerInventory);
				return;
			}

			IInventoryItem toMove;
			if (!resultSlot.IsMouseOver() || (toMove = resultSlot.GetSlotItem()) is null) return;

			playerInventory.TryAddItem(toMove);
			resultSlot.ClearItem();
		}

		public void OnMergeButtonClick() {
			if (resultSlot.IsOccupied && ItemsMoved.Remove(resultSlot.GetSlotItem())) {
				LocalGameManager.Instance.inventoryManager.playerInventory.TryAddItem(resultSlot.GetSlotItem());
				resultSlot.ClearItem();
			}

			GetNpcOwner<Smith>()
				.CmdMergeAndAddCharm(ItemsMoved.Cast<Charm>().ToArray(), LocalGameManager.Instance.LocalPlayer);
		}

		public void SetResultSlot(IInventoryItem item) {
			ItemsMoved.Add(item);
			resultSlot.SetSlotItem(item);
		}
	}
}
