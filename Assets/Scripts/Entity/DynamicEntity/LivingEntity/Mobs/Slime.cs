using System;
using Entity.EntityInterface;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Mobs
{
    public class Slime : Mob
    {
        [SerializeField] private Player target;
        [SerializeField] private int damagePerHit;

        private void Start()
        {
            InstantiateMob();
        }

        private void Move()
        {
            Vector3 position = transform.position;
            Renderer.flipX = position.x < LastPos.x + 0.2f;
            Vector2 direction = Vector2.MoveTowards(position, target.transform.position, .03f);
            if (direction.magnitude > 0.1)
            {
                Animator.SetBool("IsWalking", true);
                transform.position = direction;
                LastPos = direction;
            }
            else
                Animator.SetBool("IsWalking", false);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out IDamageable entity))
                entity.TakeDamage(damagePerHit);
        }

        private void Update()
        {
            Move();
        }
    }
}