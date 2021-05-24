using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;

namespace UI_Audio {
	public class SmithInventory: ContainerInventory {
		private const int KibryCostPerCharm = 5;
		public InventorySlot resultSlot;
		private CharmData _previewData;

		protected override bool CustomTryAdd(IInventoryItem item) {
			if (!(item is Charm charm) || !base.TryAddItem(item)) return false;
			_previewData += charm.Bonuses;
			return true;
		}

		protected override bool CustomTryRemove(IInventoryItem item) {
			if (!(item is Charm charm) || !base.TryRemoveItem(item)) return false;
			_previewData -= charm.Bonuses;
			return true;
		}

		[Command(requiresAuthority = false)]
		public void CmdMergeAndAddCharm(Charm[] toMerge, Player player, NetworkConnectionToClient sender = null) {
			if (sender != player.connectionToClient
			    || toMerge.Length <= 1
			    || !player.TryReduceKibrient(KibryCostPerCharm * (toMerge.Length - 1)))
				return;
			
			CharmData result = null;
			foreach (Charm charm in toMerge) {
				if (player.TryRemoveCharm(charm))
					result += charm.Bonuses;
			}

			if (result is null) return;
			
			player.AddCharm(LocalGameManager.Instance.weaponGenerator.GenerateCharm(result));
			TargetValidateTransaction(player.connectionToClient);
		}
	}
}
