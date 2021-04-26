using Behaviour;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob {
	public class Cow: Mob {
		protected override void RpcDying() {
			Debug.Log("Imma head out :(");
		}

		private void Start() {
			Instantiate();
		}

		private void Update() {
			if (!isServer) return;
			Player.Player[] players = FindObjectsOfType<Player.Player>();
			if (players.Length == 0) {
				if (Behaviour is Idle) return;
				Behaviour = new Idle();
			}
			Player.Player nearest = players[0];
			float distance = (Position - nearest.Position).sqrMagnitude;
			foreach (Player.Player player in players) {
				float newDistance = (Position - player.Position).sqrMagnitude;
				if (!(newDistance < distance)) continue;
				nearest = player;
				distance = newDistance;
			}
			Behaviour = new StraightFollowing(this, nearest);
		}
	}
}