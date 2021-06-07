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
		
		public new void Instantiate() {
			base.Instantiate();
			if (TryGetComponent(out RigidBody)) RigidBody.bodyType = netIdentity.isServer
				? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
		}
		
		protected abstract void Move();

		[Server] public static void SpawnProjectiles(RangedWeapon source, Vector2 position, bool fromSpecial, 
			float velocityLossPerSpawn = 0.7f) {
			Projectile projectilePrefab = source.GetProjectile();
			
			Quaternion localRotation = Quaternion.Euler(
				new Vector3(0, 0, Vector2.SignedAngle(projectilePrefab.projectileOrientation, source.orientation))
			);
			
			float initialVelocity = source.rangeData.projectileSpeedMultiplier;
			for (int i = 0; i < source.rangeData.projectileNumber; i++) {
				Projectile projectile = Instantiate(projectilePrefab, position, localRotation);
				projectile._fromWeapon = source;
				projectile.FacingDirection = source.orientation;
				projectile._fromSpecialAttack = fromSpecial;
				projectile.Speed *= initialVelocity;
				projectile.transform.localScale *= source.rangeData.projectileSizeMultiplier;
				projectile._spawnTime = Time.fixedTime;
				projectile.Instantiate();
				SetSameRenderingParameters(source, projectile);
				NetworkServer.Spawn(projectile.gameObject);
				initialVelocity *= velocityLossPerSpawn;
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
