using System;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Weapon.RangedWeapon {
	[Serializable]
	public class RangedWeaponData {
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
	}
	
	public abstract class RangedWeapon: Weapon {
		[SerializeField] protected Transform launchPoint;
		[SerializeField] private Projectile.Projectile projectile;
		
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
		
		public override RectTransform GetInformationPopup() {
			return !PlayerInfoManager.Instance 
				? null 
				: PlayerInfoManager.Instance.ShowRangedWeaponDescription(rangeData);
		}
		
		protected new void Instantiate() {
			base.Instantiate();
			if (isServer) orientation = Vector2.up;
			projectile.Instantiate();
		}

		public Projectile.Projectile GetProjectile() => projectile;

		public override string GetName() => rangeData.name;
	}
}