using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public class StoryTeller : Npc {
        [SerializeField] [Tooltip("The order matters - the first dialog will be displayed first and so on.")]
        private string[] dialogKeys;
        
        private void Start() => Instantiate();

        [Client] protected override void ClientInteract(Player player)
            => PlayerInfoManager.PrintDialog(dialogKeys, () => StopInteracting(player));
    }
}