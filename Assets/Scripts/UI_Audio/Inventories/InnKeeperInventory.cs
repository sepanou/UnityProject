using TMPro;
using UnityEngine;

namespace UI_Audio.Inventories {
    public class InnKeeperInventory : SellerInventory {
        [Header("Place to display the cost in kibrient")]
        [SerializeField] private TMP_Text kibryDisplay;
        
        private new void Start() {
            Dialog = new[] {"#innkeeper-stop"};
            base.Start();
        }

        protected override void DisplayPrice(IInventoryItem item) 
            => kibryDisplay.text = item is null ? "0" : item.GetKibryValue().ToString();
    }
}
