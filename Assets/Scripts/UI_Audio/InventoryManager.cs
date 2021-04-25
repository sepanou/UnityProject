using System;
using UnityEngine;

namespace UI_Audio {
	public class InventoryManager: MonoBehaviour {
		[NonSerialized] public static InventoryManager Instance;
		
		public Inventory playerInventory;
		public SmithInventory smithInventory;
		//public BuyerInventory buyerInventory;
		//public SellerInventory sellerInventory;

		private void Awake() {
			if (!Instance)
				Instance = this;
			else {
				// Duplicates
				Destroy(this);
				return;
			}
		}
		
		public void Initialize() {
			CloseAllInventories();
		}

		public void CloseAllInventories() {
			playerInventory.Close();
			smithInventory.Close();
		}
	}
}