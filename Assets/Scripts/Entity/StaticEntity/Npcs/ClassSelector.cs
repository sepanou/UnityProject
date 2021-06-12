using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace Entity.StaticEntity.Npcs {
    public class ClassSelector : Npc {
        [SerializeField] private PlayerClasses classType;

        private void Start() => Instantiate();

        [Client] protected override void ClientInteract(Player player) {
            switch (classType) {
                case PlayerClasses.Archer:
                    PlayerInfoManager.PrintDialog(new [] {"#archer-selector"}, () => StopInteracting(player));
                    player.CmdSwitchPlayerClass(PlayerClasses.Archer);
                    break;
                case PlayerClasses.Mage:
                    PlayerInfoManager.PrintDialog(new [] {"#mage-selector"}, () => StopInteracting(player));
                    player.CmdSwitchPlayerClass(PlayerClasses.Mage);
                    break;
                case PlayerClasses.Warrior:
                    PlayerInfoManager.PrintDialog(new [] {"#warrior-selector"}, () => StopInteracting(player));
                    player.CmdSwitchPlayerClass(PlayerClasses.Warrior);
                    break;
                default:
                    Debug.LogWarning($"[ClassSelector] Unknown player class '{classType}'!");
                    break;
            }
        }
    }
}