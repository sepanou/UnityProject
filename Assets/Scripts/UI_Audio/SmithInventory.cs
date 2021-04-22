using DataBanks;
using Entity.Collectibles;

namespace UI_Audio
{
    public class SmithInventory : Inventory
    {
        public InventorySlot resultSlot;
        private CharmData _previewData;

        public new bool TryAddItem(IInventoryItem item)
        {
            if (!(item is Charm charm)) return false;
            _previewData += charm.Bonuses;
            return base.TryAddItem(item);
        }
        
        public new bool TryRemoveItem(IInventoryItem item)
        {
            if (!(item is Charm charm) || !base.TryRemoveItem(item)) return false;
            _previewData -= charm.Bonuses;
            return true;
        }

        public void MergeCharms()
        {
            ClearInventory();
            Charm result = WeaponGeneratorDB.Instance.GenerateCharm(_previewData);
            resultSlot.SetSlotItem(result);
        }
    }
}
