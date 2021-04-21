using Entity.DynamicEntity.Weapon.MeleeWeapon;
using TMPro;
using UnityEngine;

namespace UI_Audio
{
    public class MeleeWeaponDescription : MonoBehaviour
    {
        public RectTransform rectTransform;
        [SerializeField] private TMP_Text knockBackMultiplier,
            weaponSizeMultiplier,
            defaultDamageMultiplier,
            specialDamageMultiplier,
            weaponName;

        public void SetData(MeleeWeaponData data)
        {
            knockBackMultiplier.text = "x " + data.KnockbackMultiplier;
            weaponSizeMultiplier.text = "x " + data.WeaponSizeMultiplier;
            defaultDamageMultiplier.text = "x " + data.DefaultDamageMultiplier;
            specialDamageMultiplier.text = "x " + data.SpecialDamageMultiplier;
            weaponName.text = data.Name;
        }
    }
}
