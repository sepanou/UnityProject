using Entity.DynamicEntity.Weapon.MeleeWeapon;
using TMPro;
using UnityEngine;

namespace UI_Audio {
	public class MeleeWeaponDescription: MonoBehaviour {
		public RectTransform rectTransform;
		[SerializeField] private TMP_Text knockBackMultiplier,
			weaponSizeMultiplier,
			defaultDamageMultiplier,
			specialDamageMultiplier,
			weaponName;

		public void SetData(MeleeWeaponData data) {
			knockBackMultiplier.text = "x " + data.knockbackMultiplier;
			weaponSizeMultiplier.text = "x " + data.weaponSizeMultiplier;
			defaultDamageMultiplier.text = "x " + data.defaultDamageMultiplier;
			specialDamageMultiplier.text = "x " + data.specialDamageMultiplier;
			weaponName.text = data.name;
		}
	}
}
