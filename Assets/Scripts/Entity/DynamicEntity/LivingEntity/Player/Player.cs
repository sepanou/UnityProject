using System.Collections.Generic;
using System.Net;
using DataBanks;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public enum PlayerClasses : byte
    {
        Mage,
        Warrior,
        Archer
    }
    
    public class Player : LivingEntity
    {
        public Weapon.Weapon toSpawn;
        public Vector3 toCoords;
        
        private static readonly string[] IdleAnims = {"IdleN", "IdleW", "IdleS", "IdleE"};
        private static readonly string[] WalkAnims = {"WalkN", "WalkW", "WalkS", "WalkE"};
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PlayerClassAnimators classAnimators;

        [SyncVar] public string playerName;
        [SyncVar] public PlayerClasses playerClass;
        [SyncVar] [SerializeField] protected Weapon.Weapon weapon;
        [SyncVar] private int _money = 0;
        [SyncVar] [SerializeField] private int energy;
        
        private List<Charm> _charms; // Could use targetRpc -> no need for others to see our charms !
        // serialization of Weapon objects, but it does for GameObject !
        [ShowInInspector]
        private readonly SyncList<Weapon.Weapon> _weapons = new SyncList<Weapon.Weapon>();
        private int _lastAnimationStateIndex;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            writer.WriteInt32(energy);
            writer.WriteInt32(_money);
            writer.WriteByte((byte) playerClass);
            writer.WriteString(playerName);
            writer.WriteWeapon(weapon);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            energy = reader.ReadInt32();
            _money = reader.ReadInt32();
            playerClass = (PlayerClasses) reader.ReadByte();
            playerName = reader.ReadString();
            weapon = reader.ReadWeapon();
        }
        
        private void Start()
        {
            InstantiateLivingEntity();
            _charms = new List<Charm>();
            _lastAnimationStateIndex = 0;
            SwitchPlayerClass(playerClass);
            mainCamera.gameObject.SetActive(isLocalPlayer);
            if (isLocalPlayer)
                _weapons.Callback += OnWeaponsUpdated;
        }

        // Can be executed by both client & server (Synced data analysis) -> double check
        public bool HasEnoughEnergy(int amount) => energy >= amount;
        public bool HasEnoughMoney(int amount) => _money >= amount;

        public Vector3 WorldToScreenPoint(Vector3 position)
            => mainCamera ? mainCamera.WorldToScreenPoint(position) : Vector3.zero;
        
        [ServerCallback]
        public void ReduceEnergy(int amount) => energy = amount >= energy ? 0 : energy - amount;

        private void OnWeaponsUpdated(SyncList<Weapon.Weapon>.Operation op, int itemIndex, Weapon.Weapon oldItem, Weapon.Weapon newItem)
        {
            if (op != SyncList<Weapon.Weapon>.Operation.OP_ADD) return;
            Debug.Log("New weapon added !");
            
            // Modify inventory HERE
        }

        [ServerCallback]
        private void ApplyForceToRigidBody(float x, float y)
        {
            if (!Rigibody) return;
            Vector2 direction = new Vector2(x, y);
            
            if (direction == Vector2.zero)
            {
                // Idle animations
                Rigibody.velocity = Vector2.zero;
                Animator.Play(IdleAnims[_lastAnimationStateIndex]);
                return;
            }
            // Circle divided in 4 parts -> angle measurement based on Vector2.up
            direction.Normalize();
            _lastAnimationStateIndex = (int) Vector2.SignedAngle(Vector2.up, direction) + 360;
            _lastAnimationStateIndex = _lastAnimationStateIndex / 90 % 4;
            Rigibody.velocity = GetSpeed() * direction;
            Animator.Play(WalkAnims[_lastAnimationStateIndex]);
        }

        [ServerCallback]
        private void SwitchPlayerClass(PlayerClasses @class)
        {
            // By default (when instantiating the gameObject), NetworkAnimator is disabled
            if (NetworkAnimator && NetworkAnimator.enabled && @class == playerClass) return;
            if (NetworkAnimator) NetworkAnimator.enabled = false;
            
            switch (@class)
            {
                case PlayerClasses.Archer:
                    if (Animator) Animator.runtimeAnimatorController = classAnimators.archerAnimator;
                    if (Renderer) Renderer.sprite = classAnimators.archerSprite;
                    break;
                case PlayerClasses.Warrior:
                    if (Animator) Animator.runtimeAnimatorController = classAnimators.warriorAnimator;
                    if (Renderer) Renderer.sprite = classAnimators.warriorSprite;
                    break;
                case PlayerClasses.Mage:
                    if (Animator) Animator.runtimeAnimatorController = classAnimators.archerAnimator;
                    if (Renderer) Renderer.sprite = classAnimators.archerSprite;
                    break;
                default:
                    if (Animator) Animator.runtimeAnimatorController = classAnimators.archerAnimator;
                    if (Renderer) Renderer.sprite = classAnimators.archerSprite;
                    break;
            }

            if (!Animator || !NetworkAnimator) return;
            NetworkAnimator.animator = Animator;
            StartCoroutine(NetworkAnimator.Reload());
        }

        [ServerCallback]
        private void SwitchWeapon(Weapon.Weapon newWeapon)
        {
            if (weapon) weapon.UnEquip();
            weapon = newWeapon;
            weapon.Equip(this);
        }

        [ServerCallback]
        private void CollectWeapon(Weapon.Weapon wp)
        {
            wp.holder = this;
            wp.RpcSetWeaponParent(transform);
            if (weapon)
                wp.UnEquip();
            else
                SwitchWeapon(wp);
            _weapons.Add(wp);
        }
        
        [ClientRpc]
        public void RpcCollect(uint entityNetId)
        {
            if (!NetworkIdentity.spawned.TryGetValue(entityNetId, out NetworkIdentity entityIdentity)) return;
            if (!entityIdentity.gameObject.TryGetComponent(out Entity collectible)) return;
            
            switch (collectible)
            {
                case Weapon.Weapon wp:
                    wp.isGrounded = false;
                    CollectWeapon(wp);
                    break;
                case Charm charm:
                    break;
                case Money money:
                    break;
            }
        }
        
        [ClientRpc]
        protected override void RpcDying()
        {
            Debug.Log("Player " + playerName + " is dead !");
        }
        
        // Command executed on the server
        // Only called from the client GO, on the corresponding GO on the server
        [Command]
        private void CmdMove(float x, float y)
        {
            ApplyForceToRigidBody(x, y);
        }
        
        [Command] // Only called by clients
        private void CmdAttack(bool fireOneButton, bool fireTwoButton)
        {
            if (!fireOneButton && !fireTwoButton) return;
            if (weapon && weapon.CanAttack()) weapon.UseWeapon(fireOneButton, fireTwoButton);
        }
        
        [ClientCallback]
        private void FixedUpdate()
        {
            // For physics
            if (!isLocalPlayer) return;
            CmdMove(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        [ClientCallback]
        private void Update()
        {
            // For inputs
            if (!isLocalPlayer) return;
            CmdAttack(Input.GetButtonDown("Fire1"), Input.GetButtonDown("Fire2"));
            if (netIdentity.isServer && toSpawn && Input.GetKeyDown(KeyCode.K))
            {
                NetworkServer.Spawn(Instantiate(toSpawn, toCoords, Quaternion.identity).gameObject);
                Debug.Log("Spawned !");
            }
        }
    }
    
    public static class PlayerSerialization
    {
        public static void WritePlayer(this NetworkWriter writer, Player player)
        {
            writer.WriteBoolean(player);
            if (player && player.netIdentity)
                writer.WriteNetworkIdentity(player.netIdentity);
        }

        public static Player ReadPlayer(this NetworkReader reader)
        {
            if (!reader.ReadBoolean()) return null;
            NetworkIdentity identity = reader.ReadNetworkIdentity();
            return !identity ? null : identity.GetComponent<Player>();
        }
    }
}
