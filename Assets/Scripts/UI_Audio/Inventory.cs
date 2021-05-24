using System.Collections.Generic;
using System.Linq;
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
		private readonly HashSet<IInventoryItem> _itemsMoved = new HashSet<IInventoryItem>();
		private bool _transactionCompleted;

		public override bool TryAddItem(IInventoryItem item) {
			if (!base.TryAddItem(item))
				return false;
			_itemsMoved.Add(item);
			return true;
		}

		public override bool TryRemoveItem(IInventoryItem item) {
			if (!base.TryRemoveItem(item))
				return false;
			_itemsMoved.Remove(item);
			return true;
		}

		protected abstract bool CustomTryAdd(IInventoryItem item);
		
		protected abstract bool CustomTryRemove(IInventoryItem item);

		public void TryMoveHoveredSlotItem(Inventory playerInventory) {
			InventorySlot lastHovered = InventorySlot.LastHovered;
			IInventoryItem toMove;
			if (!lastHovered || (toMove = lastHovered.GetSlotItem()) is null) 
				return;
			if (Contains(toMove)) {
				if (CustomTryRemove(toMove))
					playerInventory.TryAddItem(toMove);
			}
			else {
				if (CustomTryAdd(toMove))
					playerInventory.TryRemoveItem(toMove);
			}
		}

		public override void Close() {
			base.Close();
			ClearInventory();
			if (_transactionCompleted) {
				_transactionCompleted = false;
				return;
			}
			foreach (IInventoryItem inventoryItem in _itemsMoved)
				LocalGameManager.Instance.inventoryManager.playerInventory.TryAddItem(inventoryItem);
		}
		
		[TargetRpc] protected void TargetValidateTransaction(NetworkConnection target) => _transactionCompleted = true;
	}
}
