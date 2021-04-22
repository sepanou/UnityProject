using System;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.MeleeWeapon
{
    public class MeleeWeaponData
    {
        public float KnockbackMultiplier, WeaponSizeMultiplier;
        public float DefaultDamageMultiplier, SpecialDamageMultiplier;
        public string Name;
        
        public static MeleeWeaponData operator *(MeleeWeaponData other, int nbr)
        {
            if (other == null || nbr == 0)
                return null;
            if (nbr == 1)
                return other;
            return new MeleeWeaponData
            {
                KnockbackMultiplier = other.KnockbackMultiplier * nbr,
                WeaponSizeMultiplier = other.WeaponSizeMultiplier * nbr,
                DefaultDamageMultiplier = other.DefaultDamageMultiplier * nbr,
                SpecialDamageMultiplier = other.SpecialDamageMultiplier * nbr
            };
        }
    }
    
    public class MeleeWeapon : Weapon
    {
        [NonSerialized] public MeleeWeaponData MeleeData;
        
        private void Start() => InstantiateWeapon();

        public override RectTransform GetInformationPopup()
        {
            if (!MenuSettingsManager.Instance || !MenuSettingsManager.Instance.meleeWeaponDescription)
                return null;
            MenuSettingsManager.Instance.meleeWeaponDescription.SetData(MeleeData);
            return MenuSettingsManager.Instance.meleeWeaponDescription.rectTransform;
        }
        
        private void FixedUpdate()
        {
            // Only run by server
            if (isServer && isGrounded && !PlayerFound) GroundedLogic();
            if (!hasAuthority|| !equipped || isGrounded || !MouseCursor.Instance) return;
            // Only run by the weapon's owner (client)
            gameObject.transform.localRotation =
                MouseCursor.Instance.OrientateObjectTowardsMouse(transform.position, Vector2.up);
        }

        [ServerCallback]
        protected override void DefaultAttack()
        {
            RpcAttackAnimation();
            LastAttackTime = Time.time;
        }

        [ServerCallback]
        protected override void SpecialAttack()
        {
            RpcAttackAnimation();
            holder.ReduceEnergy(specialAttackCost);
            LastAttackTime = Time.time;
        }

        [ClientRpc] // By default, attack anims are slow -> no need for persistent NetworkAnimator
        private void RpcAttackAnimation() => Animator.Play("DefaultAttack");
    }
}
