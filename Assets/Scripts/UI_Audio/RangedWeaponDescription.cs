using Entity.DynamicEntity.Weapon.RangedWeapon;
using TMPro;
using UnityEngine;

namespace UI_Audio {
	public class RangedWeaponDescription: MonoBehaviour {
		public RectTransform rectTransform;
		
		[SerializeField] private TMP_Text projectileSpeedMultiplier,
			projectileSizeMultiplier,
			projectileNumber,
			defaultDamageMultiplier,
			specialDamageMultiplier,
			weaponName;

		public void SetData(RangedWeaponData data) {
			projectileSpeedMultiplier.text = "x " + data.projectileSpeedMultiplier;
			projectileSizeMultiplier.text = "x " + data.projectileSizeMultiplier;
			defaultDamageMultiplier.text = "x " + data.defaultDamageMultiplier;
			specialDamageMultiplier.text = "x " + data.specialDamageMultiplier;
			projectileNumber.text = data.projectileNumber + " projectiles";
			weaponName.text = data.name;
		}
	}
}
