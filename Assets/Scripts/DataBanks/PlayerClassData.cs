using System;
using Entity.DynamicEntity.LivingEntity.Player;
using UnityEngine;

namespace DataBanks
{
    [CreateAssetMenu(fileName = "PlayerClassDB", menuName = "DataBanks/PlayerClassDB", order = 1)]
    public class PlayerClassData : ScriptableObject
    { 
        public ClassData archer, warrior, mage;
    }
    
    [Serializable]
    public class ClassData
    {
        public PlayerClasses playerClass;
        public Sprite defaultSprite;
        public Sprite healthBar;
        public Sprite powerBar; // Mana for mage only
        public Sprite classIcon;
        public Sprite defaultAttackIcon, specialAttackIcon;
        public RuntimeAnimatorController animatorController;
    }
}
