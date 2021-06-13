using System;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	[Serializable]
	public class RangedWeaponData {
		public const float MaxProjectileSpeedMultiplier = 2f,
			MaxProjectileSizeMultiplier = 2f,
			MaxDefaultDamageMultiplier = 2f,
			MaxSpecialDamageMultiplier = 1.25f,
			MinProjectileSpeedMultiplier = 0.5f,
			MinProjectileSizeMultiplier = 0.5f,
			MinDefaultDamageMultiplier = 0.5f,
			MinSpecialDamageMultiplier = 0.75f;
		
		public const int MaxProjectileNumber = 4,
			MinProjectileNumber = 1;
		
		public float projectileSpeedMultiplier, projectileSizeMultiplier;
		public int projectileNumber;
		public float defaultDamageMultiplier, specialDamageMultiplier;
		public string name;

		public static RangedWeaponData operator *(RangedWeaponData other, int nbr) {
			if (other == null || nbr == 0)
				return null;
			if (nbr == 1)
				return other;
			return new RangedWeaponData {
				projectileSpeedMultiplier = other.projectileSpeedMultiplier * nbr,
				projectileSizeMultiplier = other.projectileSizeMultiplier * nbr,
				projectileNumber = other.projectileNumber * nbr,
				defaultDamageMultiplier = other.defaultDamageMultiplier * nbr,
				specialDamageMultiplier = other.specialDamageMultiplier * nbr
			};
		}
		
		public static int GetKibryValue(RangedWeaponData data) {
			float kibryValue = 0f;
			kibryValue += 100f * (data.projectileSpeedMultiplier - MinProjectileSpeedMultiplier) /
			              (MaxProjectileSpeedMultiplier - MinProjectileSpeedMultiplier);
			kibryValue += 100f * (data.projectileSizeMultiplier - MinProjectileSizeMultiplier) /
			              (MaxProjectileSizeMultiplier - MinProjectileSizeMultiplier);
			kibryValue += 100f * (data.defaultDamageMultiplier - MinDefaultDamageMultiplier) /
			              (MaxDefaultDamageMultiplier - MinDefaultDamageMultiplier);
			kibryValue += 100f * (data.specialDamageMultiplier - MinSpecialDamageMultiplier) /
			              (MaxSpecialDamageMultiplier - MinSpecialDamageMultiplier);
			kibryValue += 100f * (data.projectileNumber - MinProjectileNumber) /
			              (MaxProjectileNumber - MinProjectileNumber);
			return (int) kibryValue;
		}
	}
	
	public abstract class RangedWeapon: Weapon {
		[SerializeField] protected Transform launchPoint;
		protected Projectile.Projectile ProjectilePrefab;
		
		[SyncVar] [HideInInspector] public Vector2 orientation;
		[SyncVar] [HideInInspector] public RangedWeaponData rangeData;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteVector2(orientation);
			writer.Write(rangeData);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			orientation = reader.ReadVector2();
			rangeData = reader.Read<RangedWeaponData>();
		}

		protected abstract void SetProjectile();
		
		protected override float GetDamageMultiplier(bool isSpecial) =>
			isSpecial ? rangeData.specialDamageMultiplier : rangeData.defaultDamageMultiplier;

		public override RectTransform GetInformationPopup() 
			=> PlayerInfoManager.ShowRangedWeaponDescription(rangeData);

		public override int GetKibryValue() => RangedWeaponData.GetKibryValue(rangeData);

		protected new void Instantiate() {
			base.Instantiate();
			if (!isServer) return;
			orientation = Vector2.up;
			SetProjectile();
		}

		[Server] public Projectile.Projectile GetProjectile() => ProjectilePrefab;

		public override string GetWeaponName() => rangeData.name;
	}
}