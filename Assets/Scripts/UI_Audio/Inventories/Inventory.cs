using System.Collections.Generic;
using System.Linq;
using Entity.StaticEntity.Npcs;
using Mirror;
using UnityEngine;

public interface IInventoryItem : INetworkObject {
	bool TryGetSpriteRenderer(out SpriteRenderer renderer);
	RectTransform GetInformationPopup();
	GameObject GetGameObject();
	int GetKibryValue();
}

namespace UI_Audio.Inventories {
	public class Inventory: MonoBehaviour {
		[SerializeField] private InventorySlot[] slots;
		[SerializeField] private CanvasGroup canvasGroup;
		
		private int _count;
		public int Size { get; private set; }
		public bool IsOpen { get; private set; }

		public void Start() {
			Size = slots.Length;
			_count = 0;
		}

		protected bool Contains(IInventoryItem item)
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

	/// <summary>
	/// Inventory with a NPC owner
	/// </summary>
	public abstract class NpcInventory : Inventory {
		protected ShopKeeper NpcOwner;
		protected string[] Dialog;
		public void SetNpcOwner(ShopKeeper owner) => NpcOwner = owner;
		protected T GetNpcOwner<T>() where T : ShopKeeper => (T) NpcOwner;
		public void PrintClosingDialog() => NpcOwner.PrintDialog(Dialog, Close, true);
	}
	
	/// <summary>
	/// Inventories with the ability to exchange items with the player's inventory dynamically
	/// </summary>
	public abstract class ContainerInventory : NpcInventory {
		protected readonly HashSet<IInventoryItem> ItemsMoved = new HashSet<IInventoryItem>();

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

		public override void ClearInventory() {
			base.ClearInventory();
			ItemsMoved.Clear();
		}

		public override void Close() {
			base.Close();
			base.ClearInventory();
			
			if (!NetworkManager.singleton || !NetworkManager.singleton.isNetworkActive || !NetworkClient.active)
				return;
			
			if (NpcOwner && LocalGameManager.Instance.LocalPlayer)
				NpcOwner.StopInteracting(LocalGameManager.Instance.LocalPlayer);
			
			foreach (IInventoryItem inventoryItem in ItemsMoved)
				LocalGameManager.Instance.inventoryManager.playerInventory.TryAddItem(inventoryItem);
			
			ItemsMoved.Clear();
		}
		
	}

	/// <summary>
	/// Inventories which are opened alone (no player inventory on the side).
	/// One important point is that their inventories must be generated.
	/// </summary>
	public abstract class SellerInventory : NpcInventory {
		[SerializeField] private RectTransform infoDisplay;
		private InventorySlot _lastCostPrinted, _lastSelected;
		
		protected abstract void DisplayPrice(IInventoryItem item);

		public new void Start() {
			base.Start();
			InventorySlot.OnItemSlotClick += OnInventorySlotClick;
			DisplayPrice(null);
		}

		private void OnInventorySlotClick(InventorySlot slot) {
			if (!slot || !slot.IsOccupied || !Contains(slot.GetSlotItem())) return;
			_lastSelected = slot;
		}

		public void OnBuyButtonClick() {
			if (!_lastSelected || !_lastSelected.IsOccupied) return;
			GetNpcOwner<Seller>().CmdBuyItem(_lastSelected.GetSlotItem(), LocalGameManager.Instance.LocalPlayer);
		}
		
		public void Update() {
			if (!InventorySlot.LastHovered
			    || !InventorySlot.LastHovered.IsOccupied
			    || !InventorySlot.LastHovered.IsMouseOver()) {
				if (!_lastSelected || _lastSelected == _lastCostPrinted || !_lastSelected.IsOccupied) return;
				IInventoryItem item = _lastSelected.GetSlotItem();
				item.GetInformationPopup().gameObject.transform.SetParent(infoDisplay.transform, false);
				infoDisplay.gameObject.SetActive(true);
				DisplayPrice(item);
				_lastCostPrinted = _lastSelected;
				return;
			}
            
			DisplayPrice(InventorySlot.LastHovered.GetSlotItem());
			_lastCostPrinted = InventorySlot.LastHovered != _lastSelected ? InventorySlot.LastHovered : null;
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
