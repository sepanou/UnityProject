using System.Linq;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon;
using Entity.StaticEntity.Npcs;
using TMPro;
using UnityEngine;

namespace UI_Audio.Inventories {
    public class CollectorInventory : ContainerInventory {
        [SerializeField] private TMP_Text kibryDisplay;
        private int _kibryValue;

        private int KibryValue {
            get => _kibryValue;
            set {
                _kibryValue = value;
                kibryDisplay.text = (_kibryValue / 2).ToString(); // Half price
            }
        }

        private new void Start() {
            Dialog = new[] {"#collector-stop"};
            base.Start();
        }

        protected override bool CustomTryAdd(IInventoryItem item) {
            if (!(item is Weapon) && !(item is Charm) || !base.TryAddItem(item))
                return false;
            KibryValue += item.GetKibryValue();
            return true;
        }

        protected override bool CustomTryRemove(IInventoryItem item) {
            if (!(item is Weapon) && !(item is Charm) || !base.TryRemoveItem(item))
                return false;
            KibryValue -= item.GetKibryValue();
            return true;
        }

        public override void Close() {
            base.Close();
            KibryValue = 0;
        }

        public void ResetValue() => KibryValue = 0;

        public void OnSellButtonClick()
            => GetNpcOwner<Collector>().CmdSellItems(ItemsMoved.ToArray(), LocalGameManager.Instance.LocalPlayer);
    }
}
