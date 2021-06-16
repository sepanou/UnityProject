using UI_Audio;

namespace Entity.DynamicEntity.Projectile {
	public class MagicOrb: Projectile {
		private void Start() => Instantiate();
		
		public override void OnStartClient() {
			base.OnStartClient();
			AudioDB.PlayUISound(FromSpecialAttack ? "magicSpecialAttack" : "magicSimpleAttack");
		}
	}
}