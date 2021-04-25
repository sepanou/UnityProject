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
			else throw new Exception("created two instances of InventoryManager");
		}
		
		public void Initialize() {
			CloseAllInventories();
		}

		private void CloseAllInventories() {
			playerInventory.Close();
			smithInventory.Close();
		}
	}
}