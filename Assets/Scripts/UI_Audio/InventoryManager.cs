using System;
using Entity.DynamicEntity;
using UnityEngine;

namespace UI_Audio {
	public interface IInventoryItem {
		SpriteRenderer GetSpriteRenderer();
		RectTransform GetInformationPopup();
	}
	
	public class InventoryManager: MonoBehaviour {
		[NonSerialized] public static InventoryManager Instance;
		
		public Inventory playerInventory;
		public SmithInventory smithInventory;
		public RectTransform middlePlayerInventoryRect, middleShopInventoriesRect;
		public RectTransform sidedPlayerInventoryRect, sidedShopInventoriesRect;
		//public CollectorInventory collectorInventory;
		//public InnKeeperInventory innKeeperInventory;
		//public orchidologistInventory orchidologistInventory;

		private void Awake() {
			if (!Instance)
				Instance = this;
			else
				Destroy(this);
		}

		public void Initialize() => CloseAllInventories();

		public void CloseAllInventories() {
			sidedShopInventoriesRect.gameObject.SetActive(false);
			middleShopInventoriesRect.gameObject.SetActive(false);
			playerInventory.Close();
			smithInventory.Close();
			playerInventory.transform.SetParent(middlePlayerInventoryRect, false);
		}

		public void OpenShopKeeperInventory(Npc.NpcType shopKeeper, Npc owner) {
			CloseAllInventories();
			playerInventory.Open();

			if (shopKeeper == Npc.NpcType.InnKeeper) {
				// Middle inventories
				middleShopInventoriesRect.gameObject.SetActive(true);
				playerInventory.transform.SetParent(middlePlayerInventoryRect, false);
				return;
			}
			
			// Sided inventories
			sidedShopInventoriesRect.gameObject.SetActive(true);
			playerInventory.transform.SetParent(sidedPlayerInventoryRect, false);
			ContainerInventory toOpen;
			if (shopKeeper == Npc.NpcType.Smith) toOpen = smithInventory;
			else toOpen = null;
			
			if (toOpen != null) {
				toOpen.SetNpcOwner(owner);
				toOpen.Open();
			}
			
			LocalGameManager.Instance.LocalPlayer.SetContainerInventory(toOpen);
			// TODO the other inventories
		}
	}
}