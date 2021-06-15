using Entity;
using Entity.DynamicEntity.LivingEntity.Player;

namespace Behaviour.Targeter {
	public class SpecificTargeter<TEntity>: ITargeter where TEntity: Entity.Entity {
		public readonly TEntity target;
		public SpecificTargeter(TEntity entity) { target = entity; }
		public Entity.Entity AcquireTarget() => target;
	}
	
	public class SpecificEntityTargeter: SpecificTargeter<Entity.Entity> {
		public SpecificEntityTargeter(Entity.Entity entity): base(entity) { }
	}
	
	public class SpecificPlayerTargeter: SpecificTargeter<Player> {
		public SpecificPlayerTargeter(Player entity): base(entity) { }
	}
	
	public class SpecificMarkerTargeter: SpecificTargeter<Marker> {
		public SpecificMarkerTargeter(Marker entity): base(entity) { }
	}
}