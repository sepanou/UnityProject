using Behaviour;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public class Cow: Mob {
		protected override void RpcDying() {
			Debug.Log("Imma head out :(");
		}

		private void Start() {
			Instantiate();
			Behaviour = new NearestPlayerStraightFollower(this);
		}
	}
}