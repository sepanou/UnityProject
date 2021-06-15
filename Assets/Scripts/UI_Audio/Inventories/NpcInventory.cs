using System.Collections.Generic;
using Entity.StaticEntity.Npcs;
using Mirror;
using UnityEngine;

namespace UI_Audio.Inventories {
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
		private InventorySlot _lastSelected, _lastHovered;
		private bool _hasSelectedBeenDisplayed;
		
		// If item is null => display '0'
		public abstract void DisplayPrice(IInventoryItem item);

		protected new void Start() {
			base.Start();
			InventorySlot.OnItemSlotClick.AddListener(OnInventorySlotClick);
			InventorySlot.OnSlotHoveredChange.AddListener(OnHoveredSlotChanged);
			DisplayPrice(null);
		}

		private void OnInventorySlotClick(InventorySlot slot) {
			if (!slot || !slot.IsOccupied || !Contains(slot.GetSlotItem())) return;
			_lastSelected = slot;
			IInventoryItem item = _lastSelected.GetSlotItem();
			item.GetInformationPopup().gameObject.transform.SetParent(infoDisplay.transform, false);
			infoDisplay.gameObject.SetActive(true);
			DisplayPrice(item);
		}

		private void OnHoveredSlotChanged(InventorySlot slot) {
			if (!slot || !slot.IsOccupied) return;
			DisplayPrice(slot.GetSlotItem());
			_lastHovered = slot;
		}

		public void OnBuyButtonClick() {
			if (!_lastSelected || !_lastSelected.IsOccupied) return;
			GetNpcOwner<Seller>().CmdBuyItem(_lastSelected.GetSlotItem(), LocalGameManager.Instance.LocalPlayer);
		}
		
		public void FixedUpdate() {
			if (IsOpen && _lastSelected && _lastHovered && !_lastHovered.IsMouseOver())
				OnInventorySlotClick(_lastSelected);
		}
	}
}