namespace Entity.EntityInterface
{
    public interface IDamageable
    {
        // A damageable entity could be a player, a tree, a wall, ...
        void TakeDamage(int amount);
    }
}