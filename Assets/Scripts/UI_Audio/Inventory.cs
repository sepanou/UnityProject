using System.Linq;
using UnityEngine;

namespace UI_Audio {
	public interface IInventoryItem {
		SpriteRenderer GetSpriteRenderer();
		RectTransform GetInformationPopup();
	}
	
	public class Inventory: MonoBehaviour {
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

		public bool TryAddItem(IInventoryItem item) {
			if (_count >= _size || Contains(item)) return false;
			
			foreach (InventorySlot slot in slots) {
				if (slot.IsOccupied) continue;
				slot.SetSlotItem(item);
				++_count;
				return true;
			}
			
			return false;
		}

		public bool TryRemoveItem(IInventoryItem item) {
			foreach (InventorySlot slot in slots) {
				if (slot.GetSlotItem() != item) continue;
				slot.ClearItem();
				--_count;
				return true;
			}

			return false;
		}

		public void Open() => gameObject.SetActive(true);

		public void Close() => gameObject.SetActive(false);
	}
}
