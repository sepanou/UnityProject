using System;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using Entity.DynamicEntity.Weapon;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public class Archer : Player
    {
        private void Start()
        {
            InstantiatePlayer();
            weapon.OnSelect(this);
        }

        protected override void Dying()
        {
            Debug.Log("I'm dead :(");
        }

        protected override void Attack()
        {
            // No logic at all, just attack
            if (!weapon)
                return;
            weapon.OnUse(this);
        }

        private void Update()
        {
            Attack();
            Move();
        }
    }
}