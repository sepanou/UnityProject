using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity {
	public abstract class DynamicEntity: Entity {
		/// <summary>
		/// Can represent either a velocity (= movements) or
		/// a cooldown (= weapons) depending on the context
		/// </summary>
		[SyncVar] [SerializeField] private float speed;
		protected float DefaultSpeed { get; private set; }
		protected Animator Animator;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			writer.WriteSingle(speed);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			speed = reader.ReadSingle();
		}

		protected new void Instantiate() {
			base.Instantiate();
			Animator = GetComponent<Animator>();
			if (isServer) // The value entered in the inspector at the start is the default speed
				DefaultSpeed = speed;
		}
		
		protected float Speed {
			get => speed;
			[Server] set => speed = value <= 0f ? 0f : value >= 12.5f ? 12.5f : speed;
		}
	}
}
