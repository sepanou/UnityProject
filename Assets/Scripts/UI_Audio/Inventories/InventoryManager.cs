using System;
using Entity.DynamicEntity;
using Entity.DynamicEntity.LivingEntity.Player;
using UnityEngine;

namespace UI_Audio.Inventories {
	public interface IInventoryItem {
		SpriteRenderer GetSpriteRenderer();
		RectTransform GetInformationPopup();
	}
	
	public class InventoryManager: MonoBehaviour {
		public Inventory playerInventory;
		public RectTransform middlePlayerInventoryRect, middleShopInventoriesRect;
		public RectTransform sidedPlayerInventoryRect, sidedShopInventoriesRect;
		public SmithInventory smithInventory;
		public CollectorInventory collectorInventory;
		//public InnKeeperInventory innKeeperInventory;
		//public orchidologistInventory orchidologistInventory;

		public void Initialize() => CloseAllInventories();

		public void CloseAllInventories() {
			playerInventory.Close();
			smithInventory.Close();
			collectorInventory.Close();
			sidedShopInventoriesRect.gameObject.SetActive(false);
			middleShopInventoriesRect.gameObject.SetActive(false);
			playerInventory.transform.SetParent(middlePlayerInventoryRect, false);
		}

		private void OpenSmithInventory(Npc owner) {
			Player localPlayer = LocalGameManager.Instance.LocalPlayer;
			if (!owner.VerifyInteractionWith(localPlayer))
				return;
			
			playerInventory.Open();
			sidedShopInventoriesRect.gameObject.SetActive(true);
			playerInventory.transform.SetParent(sidedPlayerInventoryRect, false);
			smithInventory.SetNpcOwner(owner);
			smithInventory.Open();
			localPlayer.SetContainerInventory(smithInventory);
		}

		private void OpenCollectorInventory(Npc owner) {
			Player localPlayer = LocalGameManager.Instance.LocalPlayer;
			if (!owner.VerifyInteractionWith(localPlayer))
				return;
			
			playerInventory.Open();
			sidedShopInventoriesRect.gameObject.SetActive(true);
			playerInventory.transform.SetParent(sidedPlayerInventoryRect, false);
			collectorInventory.SetNpcOwner(owner);
			collectorInventory.Open();
			localPlayer.SetContainerInventory(collectorInventory);
		}

		private void OpenInnKeeperInventory(Npc owner) {
			middleShopInventoriesRect.gameObject.SetActive(true);
			playerInventory.transform.SetParent(middlePlayerInventoryRect, false);
		}

		public void OpenShopKeeperInventory(Npc.NpcType shopKeeper, Npc owner) {
			switch (shopKeeper) {
				case Npc.NpcType.Smith:
					owner.PrintDialog(new [] { "#smith-start" }, () => OpenSmithInventory(owner));
					break;
				case Npc.NpcType.Orchidologist:
					break;
				case Npc.NpcType.Collector:
					owner.PrintDialog(new [] { "#collector-start" }, () => OpenCollectorInventory(owner));
					break;
				case Npc.NpcType.InnKeeper:
					break;
				default:
					Debug.LogWarning($"{shopKeeper.ToString()} is not a shopkeeper!");
					break;
			}
		}

		public void CloseShopKeeperInventory(ContainerInventory toClose) {
			switch (toClose.NpcOwner.GetNpcType) {
				case Npc.NpcType.Smith:
					toClose.NpcOwner.PrintDialog(new[] {"#smith-stop"}, toClose.Close, true);
					break;
				case Npc.NpcType.Orchidologist:
					break;
				case Npc.NpcType.Collector:
					toClose.NpcOwner.PrintDialog(new[] {"#collector-stop"}, toClose.Close, true);
					break;
				case Npc.NpcType.InnKeeper:
					break;
				default:
					Debug.Log($"{toClose.NpcOwner.GetNpcType.ToString()} is not a shopkeeper!");
					break;
			}
			
			CloseAllInventories();
		}
	}
}