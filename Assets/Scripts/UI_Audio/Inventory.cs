using System.Collections.Generic;
using System.Linq;
using Entity.DynamicEntity;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace UI_Audio {
	public class Inventory: NetworkBehaviour {
		[SerializeField] private InventorySlot[] slots;
		private int _count, _size;
		public bool IsOpen => gameObject.activeSelf;

		private void Awake() {
			if (slots.Length == 0)
				slots = transform.GetComponentsInChildren<InventorySlot>();
			_size = slots.Length;
			_count = 0;
		}

		protected bool Contains(IInventoryItem item)
			=> slots.Any(slot => slot.GetSlotItem() == item);

		public void ClearInventory() {
			foreach (InventorySlot slot in slots)
				slot.ClearItem();
			_count = 0;
		}

		public virtual bool TryAddItem(IInventoryItem item) {
			if (_count >= _size || item is null || Contains(item)) return false;
			
			foreach (InventorySlot slot in slots) {
				if (slot.IsOccupied) continue;
				slot.SetSlotItem(item);
				++_count;
				return true;
			}
			
			return false;
		}

		public virtual bool TryRemoveItem(IInventoryItem item) {
			if (item is null) return false;
			
			foreach (InventorySlot slot in slots) {
				if (slot.GetSlotItem() != item) continue;
				slot.ClearItem();
				--_count;
				return true;
			}

			return false;
		}

		public void Open() => gameObject.SetActive(true);

		public virtual void Close() => gameObject.SetActive(false);
	}
	
	public abstract class ContainerInventory : Inventory {
		// Inventories with the ability to exchange items with the player's inventory dynamically
		protected readonly HashSet<IInventoryItem> ItemsMoved = new HashSet<IInventoryItem>();
		protected Npc NpcOwner;

		public void SetNpcOwner(Npc owner) => NpcOwner = owner;

		public override bool TryAddItem(IInventoryItem item) {
			if (!NpcOwner || !base.TryAddItem(item))
				return false;
			ItemsMoved.Add(item);
			return true;
		}

		public override bool TryRemoveItem(IInventoryItem item) {
			if (!NpcOwner || !base.TryRemoveItem(item))
				return false;
			ItemsMoved.Remove(item);
			return true;
		}

		protected abstract bool CustomTryAdd(IInventoryItem item);
		
		protected abstract bool CustomTryRemove(IInventoryItem item);

		public virtual void TryMoveHoveredSlotItem(Inventory playerInventory) {
			InventorySlot lastHovered = InventorySlot.LastHovered;
			IInventoryItem toMove;
			
			if (!lastHovered || !lastHovered.IsMouseOver() || (toMove = lastHovered.GetSlotItem()) is null)
				return;
			
			if (Contains(toMove)) {
				// Move item from containerInv to playerInv
				if (CustomTryRemove(toMove))
					playerInventory.TryAddItem(toMove);
			}
			else {
				// Move item from playerInv to containerInv
				if (CustomTryAdd(toMove))
					playerInventory.TryRemoveItem(toMove);
			}
		}

		public override void Close() {
			base.Close();
			NpcOwner = null;
			ClearInventory();
			foreach (IInventoryItem inventoryItem in ItemsMoved)
				LocalGameManager.Instance.inventoryManager.playerInventory.TryAddItem(inventoryItem);
		}

		[Server] protected bool VerifyInteractionWithNpc(Player player, Npc owner) => owner.VerifyInteractionWith(player);
	}
}
