using System;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.Collectibles
{
    public class CharmData
    {
        public float DefaultAttackDamageBonus, SpecialAttackDamageBonus;
        public int HealthBonus, PowerBonus;
        public float SpeedBonus, CooldownReduction;

        public static CharmData operator +(CharmData first, CharmData second)
        {
            if (first == null)
                return second;
            if (second == null)
                return first;
            return new CharmData
            {
                DefaultAttackDamageBonus = first.DefaultAttackDamageBonus + second.DefaultAttackDamageBonus,
                SpecialAttackDamageBonus = first.SpecialAttackDamageBonus + second.SpecialAttackDamageBonus,
                HealthBonus = first.HealthBonus + second.HealthBonus,
                PowerBonus = first.PowerBonus + second.PowerBonus,
                SpeedBonus = first.SpeedBonus + second.SpeedBonus,
                CooldownReduction = first.CooldownReduction + second.CooldownReduction
            };
        }

        public static CharmData operator -(CharmData first, CharmData second)
        {
            if (first == null)
                return second;
            if (second == null)
                return first;
            return new CharmData
            {
                DefaultAttackDamageBonus = first.DefaultAttackDamageBonus - second.DefaultAttackDamageBonus,
                SpecialAttackDamageBonus = first.SpecialAttackDamageBonus - second.SpecialAttackDamageBonus,
                HealthBonus = first.HealthBonus - second.HealthBonus,
                PowerBonus = first.PowerBonus - second.PowerBonus,
                SpeedBonus = first.SpeedBonus - second.SpeedBonus,
                CooldownReduction = first.CooldownReduction - second.CooldownReduction
            };
        }
    }
    
    public class Charm : Collectibles, IInventoryItem
    {
        [SyncVar] [NonSerialized] public CharmData Bonuses;

        private void Start() => InstantiateEntity();

        public RectTransform GetInformationPopup()
        {
            if (!MenuSettingsManager.Instance || !MenuSettingsManager.Instance.charmDescription)
                return null;
            MenuSettingsManager.Instance.charmDescription.SetData(Bonuses);
            return MenuSettingsManager.Instance.charmDescription.rectTransform;
        }
    }
}