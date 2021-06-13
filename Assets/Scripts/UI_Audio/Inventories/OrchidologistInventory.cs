using TMPro;
using UnityEngine;

namespace UI_Audio.Inventories {
    public class OrchidologistInventory : SellerInventory {
        [Header("Place to display the cost in orchids")]
        [SerializeField] private TMP_Text orchidDisplay;
        
        private new void Start() {
            Dialog = new[] {"#orchidologist-stop"};
            base.Start();
        }

        public override void DisplayPrice(IInventoryItem item) 
            => orchidDisplay.text = item is null ? "0" : (item.GetKibryValue() / 5).ToString();
    }
}