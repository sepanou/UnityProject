using System.Collections;
using Entity.DynamicEntity.LivingEntity.Mob;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.Projectile {
	public abstract class Projectile: DynamicEntity {
		[SerializeField] private Vector2 projectileOrientation;
		
		private const float LifeTime = 3f; // Maximum lifetime in seconds
		private RangedWeapon _fromWeapon;
		protected Rigidbody2D RigidBody;
		protected Vector2 FacingDirection;
		private bool _fromSpecialAttack;
		private float _spawnTime; // The time when the projectile was spawned
		
		protected new void Instantiate() {
			base.Instantiate();
			if (TryGetComponent(out RigidBody)) RigidBody.bodyType = netIdentity.isServer
				? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
		}
		
		protected abstract void Move();

		private static Projectile BuildProjectile(Projectile projectilePrefab, RangedWeapon source, Transform launchPoint, bool special) {
			Projectile projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.Euler(
				new Vector3(0, 0, Vector2.SignedAngle(projectilePrefab.projectileOrientation, source.orientation))
			));
			projectile._fromWeapon = source;
			projectile.FacingDirection = source.orientation;
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
				Projectile projectile = BuildProjectile(projectilePrefab, source, launchPoint, false);
				NetworkServer.Spawn(projectile.gameObject);
				yield return new WaitForSeconds(delayPerSpawn);
			}
		}

		// Special attack function
		[Server] public static IEnumerator WaveOfProjectiles(RangedWeapon source, Transform launchPoint, float duration) {
			Projectile projectilePrefab = source.GetProjectile();

			float startTime = Time.time;
			while (Time.time - startTime < duration) {
				Projectile projectile = BuildProjectile(projectilePrefab, source, launchPoint, true);
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
			if (other.gameObject.TryGetComponent(out Mob mob))
				mob.GetAttacked(_fromWeapon.GetDamage(_fromSpecialAttack));
			if (other.gameObject.GetComponent<Projectile>()) return;
			NetworkServer.Destroy(gameObject);
		}
	}
}
