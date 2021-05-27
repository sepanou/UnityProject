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
		[SyncVar] private int _health;

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
			_health = newHealth;
			OnHealthChange?.Invoke(_health / (float) maxHealth);
		}

		protected abstract void RpcDying();
		
		protected new void Instantiate() {
			// Physics only simulated on the server
			// On client, no collision managed but triggers still work
			//if (TryGetComponent(out Rigibody)) Rigibody.bodyType = netIdentity.isServer
			//	? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
			if (TryGetComponent(out _rigidBody)) _rigidBody.bodyType = RigidbodyType2D.Dynamic;
			LastAnimationState = 0;
			entityUI = Instantiate(entityUI, LocalGameManager.Instance.playerInfoManager.transform);
			entityUI.transform.SetSiblingIndex(0);
			entityUI.Initialize(this);
			if (isServer)
				_health = maxHealth;
			OnHealthChange?.Invoke(1f);
			entityUI.SetNameTagField(nameTag);
			base.Instantiate();
		}
		
		public void TakeKnockBack() {
			throw new NotImplementedException();
		}

		protected void ApplyForceToRigidBody(float x, float y) {
			if (!_rigidBody) return;
			Vector2 direction = new Vector2(x, y);
            			
			if (direction == Vector2.zero) {
				// Idle animations
				_rigidBody.velocity = Vector2.zero;
				Animator.Play(IdleAnims[(int) LastAnimationState]);
				return;
			}
            			
			// Circle divided in 4 parts -> angle measurement based on Vector2.up
			direction.Normalize();
			int signedAngle = (int) Vector2.SignedAngle(Vector2.up, direction);
			LastAnimationState =
				(AnimationState) ((int) Math.Round((signedAngle + 360) / 90f, 0, MidpointRounding.AwayFromZero) % 4);
			_rigidBody.velocity = Speed * direction;
			Animator.Play(WalkAnims[(int) LastAnimationState]);
			if (!advancedMoves) return;
			transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(AdvancedMoves[(int) LastAnimationState], direction));
		}

		[ClientRpc]
		protected void RpcApplyForceToRigidBody(float x, float y) {
			if (!isServer)
				ApplyForceToRigidBody(x, y);
		}

		[ServerCallback]
		public void GetAttacked(int atk) {
			if (!_isAlive || atk == 0) return;
			_health = Mathf.Max(_health - atk, 0);
			OnHealthChange?.Invoke(_health / (float) maxHealth);
			// TakeKnockback(); Needs to be implemented
			_isAlive = _health > 0;
			if (!_isAlive) RpcDying();
		}

		private void OnDestroy() {
			if (entityUI)
				entityUI.Destroy();
		}
	}
}
