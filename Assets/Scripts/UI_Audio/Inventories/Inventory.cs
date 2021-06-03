using System.Linq;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

public interface IInventoryItem : INetworkObject {
	bool TryGetSpriteRenderer(out SpriteRenderer renderer);
	RectTransform GetInformationPopup();
	GameObject GetGameObject();
	int GetKibryValue();
	void Drop(Player player);
}

namespace UI_Audio.Inventories {
	public class Inventory: MonoBehaviour {
		[SerializeField] private InventorySlot[] slots;
		[SerializeField] private CanvasGroup canvasGroup;

		private int _count;
		public int Size => slots.Length;
		public bool IsOpen { get; private set; }

		public bool Contains(IInventoryItem item)
			=> slots.Any(slot => slot.GetSlotItem() == item);

		public virtual void ClearInventory() {
			foreach (InventorySlot slot in slots)
				slot.ClearItem();
			_count = 0;
		}

		public virtual bool TryAddItem(IInventoryItem item) {
			if (_count >= Size || item is null || Contains(item)) return false;
			
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

		public void Open() {
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			IsOpen = true;
		}

		public virtual void Close() {
			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			IsOpen = false;
		}
	}

	public static class IInventoryItemSerialization {
		public static void WriteIInventoryItem(this NetworkWriter writer, IInventoryItem item) {
			bool isValid = item != null && item.GetNetworkIdentity();
			writer.WriteBoolean(isValid);
			if (isValid)
				writer.WriteNetworkIdentity(item.GetNetworkIdentity());
		}

		public static IInventoryItem ReadIInventoryItem(this NetworkReader reader) {
			if (!reader.ReadBoolean()) return null;
			NetworkIdentity identity = reader.ReadNetworkIdentity();
			return identity ? identity.GetComponent<IInventoryItem>() : null;
		}
	}
}
