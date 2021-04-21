using System;
using System.Collections.Generic;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon
{
    public class RangedWeaponData
    {
        public float ProjectileSpeedMultiplier, ProjectileSizeMultiplier;
        public int ProjectileNumber;
        public float DefaultDamageMultiplier, SpecialDamageMultiplier;
        public String Name;

        public static RangedWeaponData operator *(RangedWeaponData other, int nbr)
        {
            if (other == null || nbr == 0)
                return null;
            if (nbr == 1)
                return other;
            return new RangedWeaponData
            {
                ProjectileSpeedMultiplier = other.ProjectileSpeedMultiplier * nbr,
                ProjectileSizeMultiplier = other.ProjectileSizeMultiplier * nbr,
                ProjectileNumber = other.ProjectileNumber * nbr,
                DefaultDamageMultiplier = other.DefaultDamageMultiplier * nbr,
                SpecialDamageMultiplier = other.SpecialDamageMultiplier * nbr
            };
        }
    }
    
    public abstract class RangedWeapon : Weapon
    {
        [NonSerialized] public RangedWeaponData RangeData;
        [SerializeField] protected Transform launchPoint;
        [SerializeField] private Projectile.Projectile projectile;
        
        [SyncVar] [HideInInspector] public Vector2 orientation;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            writer.WriteVector2(orientation);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            orientation = reader.ReadVector2();
        }
        
        public override RectTransform GetInformationPopup()
        {
            if (!MenuSettingsManager.Instance || !MenuSettingsManager.Instance.rangedWeaponDescription)
                return null;
            MenuSettingsManager.Instance.rangedWeaponDescription.SetData(RangeData);
            return MenuSettingsManager.Instance.rangedWeaponDescription.rectTransform;
        }
        
        protected void InstantiateRangeWeapon()
        {
            if (isServer)
                orientation = Vector2.up;
            projectile.InstantiateProjectile();
            InstantiateWeapon();
        }

        public Projectile.Projectile GetProjectile() => projectile;
        
        // Used for name generation
        public Dictionary<int, (int, string)> weaponName=
            new Dictionary<int,(int, string)>
            {
                {0, (0, "L'arc")},                // 0 == masculine adjective, feminine otherwise.
                {1, (0, "L'arc court")},
                {2, (0, "L'arc long")},
                {3, (0, "L'arc monobloc")},
                {4, (0, "L'arc à poulies")},
                {5, (0, "L'arc droit")},
                {6, (0, "L'arc de chasse")},
                {7, (0, "L'arc Yumi")}
            };
    }
}