using System.Collections;
using Entity.DynamicEntity.LivingEntity.Mob;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using Mirror;
using UI_Audio;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Projectile: DynamicEntity {
		[SerializeField] private Vector2 projectileOrientation;
		[SerializeField] private bool isPlayerProjectile;
		
		private const float LifeTime = 3f; // Maximum lifetime in seconds
		private RangedWeapon _fromWeapon;
		private Mob _fromMob;
		private Rigidbody2D _rigidBody;
		private Vector2 _facingDirection;
		private bool _fromSpecialAttack;
		private float _spawnTime; // The time when the projectile was spawned

		protected new void Instantiate() {
			base.Instantiate();
			if (TryGetComponent(out _rigidBody)) _rigidBody.bodyType = netIdentity.isServer
				? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
		}
		
		[Server] protected virtual void Move() {
			if (_rigidBody.velocity != Vector2.zero) return;
			_rigidBody.velocity = _facingDirection * Speed;
		}

		public static Projectile BuildMobProjectile(Projectile projectilePrefab, Mob mob, Vector2 direction) {
			Projectile projectile = Instantiate(projectilePrefab, mob.transform.position, Quaternion.Euler(
				new Vector3(0, 0, Vector2.SignedAngle(projectilePrefab.projectileOrientation, direction))
			));
			projectile._fromWeapon = null;
			projectile._fromMob = mob;
			projectile._facingDirection = direction;
			projectile._spawnTime = Time.fixedTime;
			projectile.Instantiate();
			SetSameRenderingParameters(mob, projectile);
			NetworkServer.Spawn(projectile.gameObject);
			return projectile;
		}

		private static Projectile BuildWeaponProjectile(Projectile projectilePrefab, RangedWeapon source, Transform launchPoint, bool special) {
			Projectile projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.Euler(
				new Vector3(0, 0, Vector2.SignedAngle(projectilePrefab.projectileOrientation, source.orientation))
			));
			projectile._fromWeapon = source;
			projectile._fromMob = null;
			projectile._facingDirection = source.orientation;
			projectile._fromSpecialAttack = special;
			projectile.Speed *= source.rangeData.projectileSpeedMultiplier;
			projectile.transform.localScale *= source.rangeData.projectileSizeMultiplier;
			projectile._spawnTime = Time.fixedTime;
			projectile.Instantiate();
			SetSameRenderingParameters(source, projectile);
			return projectile;
		}
		
		// Default attack function
		[Server] public static IEnumerator SpawnProjectiles(RangedWeapon source, Transform launchPoint, float delayPerSpawn = 0.1f) {
			Projectile projectilePrefab = source.GetProjectile();
			for (int i = 0; i < source.rangeData.projectileNumber; i++) {
				if (source is Bow _) AudioDB.PlayUISound("shotArrow");
				else AudioDB.PlayUISound("magicSimpleAttack"); // Wizard
				Projectile projectile = BuildWeaponProjectile(projectilePrefab, source, launchPoint, false);
				NetworkServer.Spawn(projectile.gameObject);
				yield return new WaitForSeconds(delayPerSpawn);
			}
		}

		// Special attack function
		[Server] public static IEnumerator WaveOfProjectiles(RangedWeapon source, Transform launchPoint, float duration) {
			Projectile projectilePrefab = source.GetProjectile();

			float startTime = Time.time;
			while (Time.time - startTime < duration) {
				if (source is Bow _) AudioDB.PlayUISound("shotArrow");
				else AudioDB.PlayUISound("magicSpecialAttack"); // Wizard
				Projectile projectile = BuildWeaponProjectile(projectilePrefab, source, launchPoint, true);
				NetworkServer.Spawn(projectile.gameObject);
				yield return new WaitForSeconds(0.2f);
			}
		}

		[ServerCallback] private void FixedUpdate() {
			if (Time.fixedTime - _spawnTime > LifeTime) {
				NetworkServer.Destroy(gameObject);
				return;
			}
			
			Move();
		}
		
		[ServerCallback] private void OnCollisionEnter2D(Collision2D other) {
			if (_fromWeapon && other.gameObject.TryGetComponent(out Mob mob))
				mob.GetAttacked(_fromWeapon.GetDamage(_fromSpecialAttack));
			else if (_fromMob && other.gameObject.TryGetComponent(out Player player))
				player.GetAttacked(_fromMob.atk);
			else if (_fromWeapon && other.gameObject.TryGetComponent(out Player _))
				return;
			else if (_fromMob && other.gameObject.TryGetComponent(out Mob _))
				return;
			
			NetworkServer.Destroy(gameObject);
		}
	}
}
