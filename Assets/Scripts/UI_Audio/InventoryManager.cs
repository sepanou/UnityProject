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
		public RectTransform sidedPlayerInventoryRect, sidedShopInventoryRect;
		//public CollectorInventory collectorInventory;
		//public InnKeeperInventory innKeeperInventory;
		//public orchidologistInventory orchidologistInventory;

		private void Awake() {
			if (!Instance)
				Instance = this;
			else
				Destroy(this);
		}
		
		public void Initialize() {
			CloseAllInventories();
		}

		public void CloseAllInventories() {
			playerInventory.Close();
			smithInventory.Close();
		}

		public void OpenPlayerAnd(Npc.NpcType otherInventory) {
			playerInventory.transform.SetParent(sidedPlayerInventoryRect);
			switch (otherInventory) {
				case Npc.NpcType.Collector:
					break;
				case Npc.NpcType.Orchidologist:
					break;
				case Npc.NpcType.Smith:
					smithInventory.transform.SetParent(sidedShopInventoryRect);
					smithInventory.Open();
					break;
				default:
					Debug.Log("This NPC does not require to display the player's inventory !");
					break;
			}
			playerInventory.Open();
		}
	}
}