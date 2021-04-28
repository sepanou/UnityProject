using System;
using System.Collections.Generic;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	public class RangedWeaponData {
		public float ProjectileSpeedMultiplier, ProjectileSizeMultiplier;
		public int ProjectileNumber;
		public float DefaultDamageMultiplier, SpecialDamageMultiplier;
		public string Name;

		public static RangedWeaponData operator *(RangedWeaponData other, int nbr) {
			if (other == null || nbr == 0)
				return null;
			if (nbr == 1)
				return other;
			return new RangedWeaponData {
				ProjectileSpeedMultiplier = other.ProjectileSpeedMultiplier * nbr,
				ProjectileSizeMultiplier = other.ProjectileSizeMultiplier * nbr,
				ProjectileNumber = other.ProjectileNumber * nbr,
				DefaultDamageMultiplier = other.DefaultDamageMultiplier * nbr,
				SpecialDamageMultiplier = other.SpecialDamageMultiplier * nbr
			};
		}
	}
	
	public abstract class RangedWeapon: Weapon {
		[NonSerialized] public RangedWeaponData RangeData;
		[SerializeField] protected Transform launchPoint;
		[SerializeField] private Projectile.Projectile projectile;
		
		[SyncVar] [HideInInspector] public Vector2 orientation;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteVector2(orientation);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			orientation = reader.ReadVector2();
		}
		
		public override RectTransform GetInformationPopup() {
			return !PlayerInfoManager.Instance 
				? null 
				: PlayerInfoManager.Instance.ShowRangedWeaponDescription(RangeData);
		}
		
		protected new void Instantiate() {
			if (isServer)
				orientation = Vector2.up;
			projectile.Instantiate();
			base.Instantiate();
		}

		public Projectile.Projectile GetProjectile() => projectile;

		public override string GetName() => RangeData.Name;
	}
}