using Entity.DynamicEntity.LivingEntity.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio.LivingEntityUI {
    public class PlayerUI : LivingEntityUI
    {
        [SerializeField] private Slider energyBar;
        [SerializeField] private Image energyImage;
        
        public void SetEnergyBarColor(PlayerClasses @class)
            => energyImage.color = @class == PlayerClasses.Archer || @class == PlayerClasses.Warrior
                ? Color.green
                : Color.blue;
        
        public void SetEnergyBarValue(float amount) => energyBar.value = amount;
    }
}
