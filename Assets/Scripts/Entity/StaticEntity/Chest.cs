using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon.MeleeWeapon;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entity.StaticEntity{
    public class Chest: Entity, IInteractiveEntity {
        private bool _isOpen;
        [SerializeField] private Sprite spriteOpened;
        private int _count;
        private Vector3 _chestCoordinates;
        [SerializeField] private bool starterChest;

        public void Start() {
            Instantiate();
            _isOpen = false;
            _chestCoordinates = transform.position;
            _count = 1;
        }

        public override void OnStartClient() {
            base.OnStartClient();
            CmdSynchronizePosition();
        }

        [ClientRpc] private void RpcToggleSprite() => spriteRenderer.sprite = spriteOpened;

        [Server] private void GenerateLoot(Player player) {
            if (!starterChest && Random.Range(0,9) < 5) GenerateCharm();
            else {
                switch (player.playerClass) {
                    case PlayerClasses.Archer:
                        GenerateBow();
                        break;
                    case PlayerClasses.Warrior:
                        GenerateSword();
                        break;
                    case PlayerClasses.Mage:
                        GenerateStaff();
                        break;
                }
            }
            ++_count;
        }

        [Server] private void GenerateBow() {
            Bow bow = WeaponGenerator.GenerateBow(Random.Range(0, 9) < 1);
            bow.transform.position = _chestCoordinates + new Vector3(_count % 3 - 1, -1, 0);
            NetworkServer.Spawn(bow.gameObject);
        }

        [Server] private void GenerateSword() {
            MeleeWeapon meleeWeapon = WeaponGenerator.GenerateSword(Random.Range(0, 9) < 1);
            meleeWeapon.transform.position = _chestCoordinates + new Vector3(_count % 3 - 1, -1, 0);
            NetworkServer.Spawn(meleeWeapon.gameObject);

        }

        [Server] private void GenerateStaff() {
            Staff staff = WeaponGenerator.GenerateStaff(Random.Range(0, 9) < 1);
            staff.transform.position = _chestCoordinates + new Vector3(_count % 3 - 1, -1, 0);
            NetworkServer.Spawn(staff.gameObject);
        }
        
        [Server] private void GenerateCharm() {
            Charm charm = WeaponGenerator.GenerateCharm();
            charm.transform.position = _chestCoordinates + new Vector3(_count % 3 - 1, -1, 0);
            NetworkServer.Spawn(charm.gameObject);
        }

        [Server] public void Interact(Player player) {
            if (_isOpen) return;
            RpcToggleSprite();
            CustomNetworkManager customNetworkManager = CustomNetworkManager.Instance;
            customNetworkManager.PlayerPrefabs.ForEach(GenerateLoot);
            _isOpen = true;
        }
    }
}