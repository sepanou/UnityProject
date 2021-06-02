using UnityEngine;

namespace UI_Audio.Inventories {
	public class InventoryManager: MonoBehaviour {
		public Inventory playerInventory;
		public RectTransform middlePlayerInventoryRect, middleShopInventoriesRect;
		public RectTransform sidedPlayerInventoryRect, sidedShopInventoriesRect;
		public SmithInventory smithInventory;
		public CollectorInventory collectorInventory;
		public InnKeeperInventory innKeeperInventory;
		//public orchidologistInventory orchidologistInventory;

		public void Initialize() => CloseAllInventories();

		public void CloseAllInventories() {
			playerInventory.Close();
			smithInventory.Close();
			collectorInventory.Close();
			innKeeperInventory.Close();
			sidedShopInventoriesRect.gameObject.SetActive(false);
			middleShopInventoriesRect.gameObject.SetActive(false);
			playerInventory.transform.SetParent(middlePlayerInventoryRect, false);
		}

		public void ClearAllInventories() {
			playerInventory.ClearInventory();
			smithInventory.ClearInventory();
			collectorInventory.ClearInventory();
			innKeeperInventory.ClearInventory();
		}

		public void OpenSidedInventory() {
			playerInventory.Open();
			sidedShopInventoriesRect.gameObject.SetActive(true);
			playerInventory.transform.SetParent(sidedPlayerInventoryRect, false);
		}

		public void OpenMiddleInventory() => middleShopInventoriesRect.gameObject.SetActive(true);

		public void CloseShopKeeperInventory(NpcInventory toClose) {
			toClose.PrintClosingDialog();
			CloseAllInventories();
		}
	}
}