using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mob
{
    public abstract class Mob : LivingEntity
    {
        protected Vector2 LastPos;

        protected void InstantiateMob()
        {
            LastPos = transform.position;
            InstantiateLivingEntity();
        }

        protected abstract void MobPathFinding(); // Maxence au Boulot
    }
}