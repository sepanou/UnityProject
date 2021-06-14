using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;

namespace Entity.Collectibles {
	public class Kibry: Collectibles {
		public int amount;
		private bool _playerFound;
		
		private new void Instantiate() {
			base.Instantiate();
			AutoStopInteracting = true;
			InteractionCondition = player => !_playerFound;
		}

		private void Start() => Instantiate();

		[Server] public override void Interact(Player player) {
			StartCoroutine(OnTargetDetected(this, player));
			_playerFound = true;
		}
	}
}