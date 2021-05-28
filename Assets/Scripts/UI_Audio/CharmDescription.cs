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
			defaultAttackBonus.text = "+ " + data.defaultAttackDamageBonus + " %";
			specialAttackBonus.text = "+ " + data.specialAttackDamageBonus + " %";
			healthBonus.text = "+ " + data.healthBonus + " HP";
			powerBonus.text = "+ " + data.powerBonus + " PP";
			cooldownReduction.text = "- " + data.cooldownReduction + " %";
			speedBonus.text = "+ " + data.speedBonus + " %";
		}
	}
}
