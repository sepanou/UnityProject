using Behaviour;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public class Cow: Mob {
		private readonly DistanceNearestPlayerStraightFollower moving;
		[SerializeField] private int atk = 15;
		[SerializeField] private int cooldown = 60;
		private int cooldownCount = 0;

		public Cow() {
			moving = new DistanceNearestPlayerStraightFollower(this, 0.5f, 8);
		}

		private void Start() {
			Instantiate();
		}

		[ServerCallback]
		private void Update() {
			if (ReferenceEquals(behaviour, moving)) {
				if (moving.SqrDistance > 1) return;
				if (cooldownCount == 0)
					moving.behaviour.targeter.target.GetAttacked(atk);
				cooldownCount = (cooldownCount + 1) % cooldown;
			} else {
				behaviour = moving;
			}
		}
	}
}