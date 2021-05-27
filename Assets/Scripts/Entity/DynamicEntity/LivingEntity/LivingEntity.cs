using System;
using Mirror;
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
		
		[SyncVar] [SerializeField] private int maxHealth;
		[SyncVar(hook = nameof(SyncHealthChanged))] private int _health;
		private void SyncHealthChanged(int o, int n) => OnHealthChange?.Invoke(n / (float) maxHealth);

		public event HealthChanged OnHealthChange;
		public delegate void HealthChanged(float ratio);
		public AnimationState LastAnimationState { get; private set; }
		private Rigidbody2D _rigidBody;
		private bool _isAlive = true;

		[SerializeField] private bool advancedMoves;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteInt32(maxHealth);
			writer.WriteInt32(_health);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			maxHealth = reader.ReadInt32();
			int newHealth = reader.ReadInt32();
			if (newHealth == _health) return;
			SyncHealthChanged(_health, newHealth);
			_health = newHealth;
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
			OnHealthChange += entityUI.SetHealthBarValue;
			if (isServer)
				_health = maxHealth;
			SyncHealthChanged(_health, _health);
			entityUI.SetNameTagField(nameTag);
		}
		
		public void TakeKnockBack() {
			throw new NotImplementedException();
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
			if (!_isAlive || atk == 0) return;
			_health = Mathf.Max(_health - atk, 0);
			SyncHealthChanged(_health, _health);
			// TakeKnockback(); Needs to be implemented
			_isAlive = _health > 0;
			if (!_isAlive) RpcDying();
		}

		private void OnDestroy() {
			if (entityUI) entityUI.Destroy();
		}
	}
}
