using System;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.PlayerClass
{
    public class Archer : Player
    {
        private int stamina;
        private void Start()
        {
            InstantiatePlayer();
        }

        /* private void Update()
        {
            Move();
        } */
    }
}
