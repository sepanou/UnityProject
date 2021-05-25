using System;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity {
	public abstract class LivingEntity: DynamicEntity {
		private static readonly string[] IdleAnims = {"IdleN", "IdleW", "IdleS", "IdleE"};
		private static readonly string[] WalkAnims = {"WalkN", "WalkW", "WalkS", "WalkE"};
		private static readonly Vector2[] AdvancedMoves = {Vector2.up, Vector2.left, Vector2.down, Vector2.right};
		
		[SyncVar] private float _health;
		private bool _isAlive = true;
		private int _lastAnimationStateIndex;
		private Rigidbody2D _rigidbody;

		[SerializeField] private bool advancedMoves;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteSingle(_health);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			_health = reader.ReadSingle();
		}

		protected abstract void RpcDying();
		
		protected new void Instantiate() {
			// Physics only simulated on the server
			// On client, no collision managed but triggers still work
			//if (TryGetComponent(out Rigibody)) Rigibody.bodyType = netIdentity.isServer
			//	? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
			if (TryGetComponent(out _rigidbody)) _rigidbody.bodyType = RigidbodyType2D.Dynamic;
			_health = 20;
			_lastAnimationStateIndex = 0;
			base.Instantiate();
		}
		
		public void TakeKnockBack() {
			throw new NotImplementedException();
		}

		protected void ApplyForceToRigidBody(float x, float y) {
			if (!_rigidbody) return;
			Vector2 direction = new Vector2(x, y);
            			
			if (direction == Vector2.zero) {
				// Idle animations
				_rigidbody.velocity = Vector2.zero;
				Animator.Play(IdleAnims[_lastAnimationStateIndex]);
				return;
			}
            			
			// Circle divided in 4 parts -> angle measurement based on Vector2.up
			direction.Normalize();
			int signedAngle = (int) Vector2.SignedAngle(Vector2.up, direction);
			_lastAnimationStateIndex = (int) Math.Round((signedAngle + 360) / 90f, 0, MidpointRounding.AwayFromZero) % 4;
			_rigidbody.velocity = Speed * direction;
			Animator.Play(WalkAnims[_lastAnimationStateIndex]);
			if (!advancedMoves) return;
			transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(AdvancedMoves[_lastAnimationStateIndex], direction));
		}

		[ClientRpc]
		protected void RpcApplyForceToRigidBody(float x, float y) {
			if (!isServer)
				ApplyForceToRigidBody(x, y);
		}

		[ServerCallback]
		public void GetAttacked(int atk) {
			if (!_isAlive) return;
			_health -= atk;
			// TakeKnockback(); Needs to be implemented
			_isAlive = _health > 0;
			if (!_isAlive) RpcDying();
		}

		[ServerCallback]
		protected void ChangeHealth(float damages) => _health = Mathf.Max(_health + damages, 0);

		[ServerCallback]
		protected void SetHealth(float health) => health = Mathf.Max(health, 0);
	}
}
