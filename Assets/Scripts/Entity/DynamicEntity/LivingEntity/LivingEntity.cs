using System;
using Mirror;
using UI_Audio;
using UI_Audio.LivingEntityUI;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity {
	public enum AnimationState {
		North = 0,
		West = 1,
		South = 2,
		East = 3
	}
	
	public abstract class LivingEntity: DynamicEntity {
		private static readonly string[] IdleAnims = {"IdleN", "IdleW", "IdleS", "IdleE"};
		private static readonly string[] WalkAnims = {"WalkN", "WalkW", "WalkS", "WalkE"};
		private static readonly Vector2[] AdvancedMoves = {Vector2.up, Vector2.left, Vector2.down, Vector2.right};
		
		[Header("LivingEntity Fields")]
		[SerializeField] protected LivingEntityUI entityUI;
		[SerializeField] private string nameTag;
		
		protected int DefaultMaxHealth { get; private set; }
		[SyncVar(hook = nameof(SyncHealthChanged))] [SerializeField] protected int maxHealth;
		[SyncVar(hook = nameof(SyncHealthChanged))] [ShowInInspector] protected int Health;
		private void SyncHealthChanged(int o, int n) => OnHealthChange?.Invoke(Health / (float) maxHealth);
		
		public readonly CustomEvent<LivingEntity> OnEntityDie = new CustomEvent<LivingEntity>();
		public AnimationState LastAnimationState { get; private set; }
		protected readonly CustomEvent<float> OnHealthChange = new CustomEvent<float>();
		private Rigidbody2D _rigidBody;
		public bool IsAlive => Health > 0;

		[SerializeField] private bool advancedMoves;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteInt32(maxHealth);
			writer.WriteInt32(Health);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			int newMaxHealth = reader.ReadInt32();
			int newHealth = reader.ReadInt32();
			if (newHealth == Health && newMaxHealth == maxHealth) return;
			maxHealth = newMaxHealth;
			SyncHealthChanged(Health, newHealth);
			Health = newHealth;
		}

		protected abstract void RpcDying();
		
		protected new void Instantiate() {
			base.Instantiate();
			if (TryGetComponent(out _rigidBody)) {
				_rigidBody.bodyType = RigidbodyType2D.Dynamic;
				_rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
			}
			
			LastAnimationState = 0;
			entityUI = Instantiate(entityUI, PlayerInfoManager.transform);
			entityUI.transform.SetSiblingIndex(0);
			entityUI.Initialize(this);
			OnHealthChange.AddListener(entityUI.SetHealthBarValue);
			
			if (isServer) {
				DefaultMaxHealth = maxHealth;
				Health = maxHealth;
			}
			
			SyncHealthChanged(Health, Health);
			entityUI.SetNameTagField(nameTag);
		}
		
		/// <summary>
		/// Apply a knockback force resulting of an attack, with a given velocity.
		/// </summary>
		/// <param name="source">The location where the attack has been performed</param>
		/// <param name="velocity">How much should the entity back up?</param>
		[Server] public void TakeKnockBack(Vector3 source, float velocity) {
			Vector2 knockbackDirection = transform.position - source;
			knockbackDirection.Normalize();
			_rigidBody.AddForce(knockbackDirection * velocity, ForceMode2D.Impulse);
		}

		[ClientRpc] private void RpcApplyAnimationStates(int animationState, bool isIdle, Vector2 velocity) {
			if (!Animator || !_rigidBody) return;
			Animator.Play(isIdle ? IdleAnims[animationState] : WalkAnims[animationState]);
			_rigidBody.velocity = velocity;
		}
		
		[Server] private (AnimationState, bool, Vector2) ApplyForceToRigidBody(float x, float y) {
			if (!_rigidBody) return (0, true, Vector2.zero);
			Vector2 direction = new Vector2(x, y);
            			
			if (direction == Vector2.zero) {
				// Idle animations
				_rigidBody.velocity = Vector2.zero;
				Animator.Play(IdleAnims[(int) LastAnimationState]);
				return (LastAnimationState, true, Vector2.zero);
			}
            			
			// Circle divided in 4 parts -> angle measurement based on Vector2.up
			direction.Normalize();
			int signedAngle = (int) Vector2.SignedAngle(Vector2.up, direction);
			LastAnimationState =
				(AnimationState) ((int) Math.Round((signedAngle + 360) / 90f, 0, MidpointRounding.AwayFromZero) % 4);
			_rigidBody.velocity = Speed * direction;
			Animator.Play(WalkAnims[(int) LastAnimationState]);
			if (advancedMoves)
				transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(AdvancedMoves[(int) LastAnimationState], direction));
			return (LastAnimationState, false, _rigidBody.velocity);
		}
		
		[Command] protected void CmdMove(float x, float y) => Move(x, y);
		
		[Server] protected void Move(float x, float y) {
			(AnimationState state, bool isIdle, Vector2 velocity) = ApplyForceToRigidBody(x, y);
			RpcApplyAnimationStates((int) state, isIdle, velocity);
		}

		[Server] public void GetAttacked(int atk) {
			if (!IsAlive || atk == 0) return;
			Health = Mathf.Max(Health - atk, 0);
			SyncHealthChanged(Health, Health);
			AudioDB.PlayUISound("damageTaken");
			if (IsAlive) return;
			OnEntityDie?.Invoke(this);
			RpcDying();
		}

		private void OnDestroy() {
			if (entityUI && entityUI.isActiveAndEnabled) entityUI.Destroy();
		}
	}
}
