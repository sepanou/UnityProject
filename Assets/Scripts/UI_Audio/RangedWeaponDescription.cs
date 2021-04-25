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
			projectileSpeedMultiplier.text = "x " + data.ProjectileSpeedMultiplier;
			projectileSizeMultiplier.text = "x " + data.ProjectileSizeMultiplier;
			defaultDamageMultiplier.text = "x " + data.DefaultDamageMultiplier;
			specialDamageMultiplier.text = "x " + data.SpecialDamageMultiplier;
			projectileNumber.text = data.ProjectileNumber + " projectiles";
			weaponName.text = data.Name;
		}
	}
}
