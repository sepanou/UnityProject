using Entity.Collectibles;
using TMPro;
using UnityEngine;

namespace UI_Audio {
	public class CharmDescription: MonoBehaviour {
		public RectTransform rectTransform;
		[SerializeField] private TMP_Text defaultAttackBonus,
			specialAttackBonus,
			healthBonus, powerBonus,
			speedBonus, cooldownReduction;

		public void SetData(CharmData data) {
			defaultAttackBonus.text = "+ " + data.DefaultAttackDamageBonus + " %";
			specialAttackBonus.text = "+ " + data.SpecialAttackDamageBonus + " %";
			healthBonus.text = "+ " + data.HealthBonus + " HP";
			powerBonus.text = "+ " + data.PowerBonus + " PP";
			cooldownReduction.text = "- " + data.CooldownReduction + " %";
			speedBonus.text = "+ " + data.SpeedBonus + " %";
		}
	}
}
