using System.Linq;
using Entity.Collectibles;
using Entity.DynamicEntity;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;

namespace UI_Audio.Inventories {
	public class SmithInventory: ContainerInventory {
		private const int KibryCostPerCharm = 5;
		public InventorySlot resultSlot;

		[Client] protected override bool CustomTryAdd(IInventoryItem item) => item is Charm && base.TryAddItem(item);
		
		[Client] protected override bool CustomTryRemove(IInventoryItem item) => item is Charm && base.TryRemoveItem(item);

		public override void Close() {
			base.Close();
			resultSlot.ClearItem();
		}

		[Client] public override void TryMoveHoveredSlotItem(Inventory playerInventory) {
			if (!InventorySlot.LastHovered) return;
			
			if (InventorySlot.LastHovered != resultSlot) {
				base.TryMoveHoveredSlotItem(playerInventory);
				return;
			}

			IInventoryItem toMove;
			if (!resultSlot.IsMouseOver() || (toMove = resultSlot.GetSlotItem()) is null) return;

			playerInventory.TryAddItem(toMove);
			resultSlot.ClearItem();
		}

		[TargetRpc] private void TargetMergeSuccessful(NetworkConnection target, Charm finalCharm) {
			ItemsMoved.Clear();
			ClearInventory();
			
			if (!LocalGameManager.Instance.inventoryManager.playerInventory.TryRemoveItem(finalCharm))
				return; // Should not happen
			
			NpcOwner.PrintDialog(new[] { "#trade-completed", "#want-more" }, null, true);
			ItemsMoved.Add(finalCharm);
			resultSlot.SetSlotItem(finalCharm);
		}

		[Command(requiresAuthority = false)]
		private void CmdMergeAndAddCharm(Charm[] toMerge, Npc owner, Player player, NetworkConnectionToClient sender = null) {
			// Check for cheats, potential incorrect args and the possibility to proceed...
			if (sender != player.connectionToClient) return;
			
			if (!VerifyInteractionWithNpc(player, owner)) {
				player.TargetPrintWarning(sender, "You are no longer interacting with this NPC!");
				return;
			}
			
			if (toMerge.Length <= 1) {
				player.TargetPrintInfoMessage(sender, "Not enough charms to merge - add more");
				return;
			}
			
			if (!player.TryReduceKibrient(KibryCostPerCharm * (toMerge.Length - 1))) {
				player.TargetPrintInfoMessage(sender, "You need more Kibrient...");
				return;
			}
			
			CharmData result = null;
			foreach (Charm charm in toMerge) {
				if (!player.RemoveCharm(charm)) continue;
				result += charm.bonuses;
				NetworkServer.Destroy(charm.gameObject);
			}

			if (result is null) return;
			
			Charm finalCharm = LocalGameManager.Instance.weaponGenerator.GenerateCharm(result);
			finalCharm.DisableInteraction(player);
			finalCharm.SetIsGrounded(false);
			finalCharm.GetSpriteRenderer().enabled = false;
			finalCharm.transform.parent = player.transform;
			NetworkServer.Spawn(finalCharm.gameObject);
			player.AddCharm(finalCharm);

			TargetMergeSuccessful(sender, finalCharm);
		}

		[Client] public void OnMergeButtonClick() =>
			CmdMergeAndAddCharm(ItemsMoved.Cast<Charm>().ToArray(), NpcOwner, LocalGameManager.Instance.LocalPlayer);
	}
}
