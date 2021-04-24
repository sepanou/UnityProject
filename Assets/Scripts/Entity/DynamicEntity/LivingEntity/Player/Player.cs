using System;
using System.Collections.Generic;
using DataBanks;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UI_Audio;
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
        public static event LocalPlayerClassChanged OnLocalPlayerClassChange;
        public static event RemotePlayerClassChanged OnRemotePlayerClassChange;
        [NonSerialized] public static InputManager InputManager;
        public delegate void LocalPlayerClassChanged(ClassData data);

        public delegate void RemotePlayerClassChanged(ClassData data);
        
        [SerializeField] private PlayerClassData classData;

        [SyncVar] public string playerName;
        [SyncVar] public PlayerClasses playerClass;
        [SyncVar] [SerializeField] protected Weapon.Weapon weapon;
        [SyncVar] private int _kibrient;
        [SyncVar] private int _orchid;
        [SyncVar] [SerializeField] private int energy;

        private Camera _mainCamera;

        private List<Charm> _charms; // Could use targetRpc -> no need for others to see our charms !
        // serialization of Weapon objects, but it does for GameObject !
        [ShowInInspector]
        private readonly SyncList<Weapon.Weapon> _weapons = new SyncList<Weapon.Weapon>();
        private Inventory _inventory;

        public int Kibrient => _kibrient;
        public int Orchid => _orchid;

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            base.OnSerialize(writer, initialState);
            writer.WriteInt32(energy);
            writer.WriteInt32(_kibrient);
            writer.WriteInt32(_orchid);
            writer.WriteByte((byte) playerClass);
            writer.WriteString(playerName);
            writer.WriteWeapon(weapon);
            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
            energy = reader.ReadInt32();
            _kibrient = reader.ReadInt32();
            _orchid = reader.ReadInt32();
            playerClass = (PlayerClasses) reader.ReadByte();
            playerName = reader.ReadString();
            weapon = reader.ReadWeapon();
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            InstantiateLivingEntity();
            _charms = new List<Charm>();
            OnLocalPlayerClassChange += ChangeAnimator;
            OnRemotePlayerClassChange += ChangeAnimator;
            if (!isLocalPlayer) return;
            _inventory = InventoryManager.Instance.playerInventory;
            _mainCamera = LocalGameManager.Instance.SetMainCameraToPlayer(this);
            _weapons.Callback += OnWeaponsUpdated;
            PlayerInfoManager.Instance.UpdateMoneyAmount(this);
            SwitchClass(playerClass);
        }

        // Can be executed by both client & server (Synced data analysis) -> double check
        public bool HasEnoughEnergy(int amount) => energy >= amount;
        
        public bool HasEnoughKibrient(int amount) => _kibrient >= amount;
        
        public bool HasEnoughOrchid(int amount) => _orchid >= amount;

        public Vector3 WorldToScreenPoint(Vector3 position)
            => _mainCamera ? _mainCamera.WorldToScreenPoint(position) : Vector3.zero;

        private void ChangeAnimator(ClassData data)
        {
            if (Animator) Animator.runtimeAnimatorController = data.animatorController;
            if (Renderer) Renderer.sprite = data.defaultSprite;
            playerClass = data.playerClass;
        }
        
        private void SwitchClass(PlayerClasses @class)
        {
            ClassData data = @class == PlayerClasses.Warrior ? classData.warrior :
                @class == PlayerClasses.Mage ? classData.mage : classData.archer;

            if (isLocalPlayer)
                OnLocalPlayerClassChange?.Invoke(data);
            else
                OnRemotePlayerClassChange?.Invoke(data);
        }

        private void OnWeaponsUpdated(SyncList<Weapon.Weapon>.Operation op, int itemIndex, Weapon.Weapon oldItem, Weapon.Weapon newItem)
        {
            if (!isLocalPlayer) return;
            
            switch (op)
            {
                case SyncList<Weapon.Weapon>.Operation.OP_ADD:
                    _inventory.TryAddItem(newItem);
                    break;
                case SyncList<Weapon.Weapon>.Operation.OP_CLEAR:
                    _inventory.ClearInventory();
                    break;
                case SyncList<Weapon.Weapon>.Operation.OP_REMOVEAT:
                    _inventory.TryRemoveItem(oldItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }
        
        [ServerCallback] public void ReduceEnergy(int amount) => energy = amount >= energy ? 0 : energy - amount;

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
            wp.netIdentity.AssignClientAuthority(netIdentity.connectionToClient);
            wp.holder = this;
            wp.RpcSetWeaponParent(transform);
            if (weapon)
                wp.UnEquip();
            else
                SwitchWeapon(wp);
            _weapons.Add(wp);
        }

        [ClientRpc] private void RpcSwitchPlayerClass(PlayerClasses @class) => SwitchClass(@class);

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
                    if (wp.TryGetComponent(out NetworkTransform netTransform))
                        netTransform.clientAuthority = true;
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
        
        [Command]
        public void CmdSwitchPlayerClass(PlayerClasses @class)
        {
            if (@class == playerClass) return;
            RpcSwitchPlayerClass(@class);
        }
        
        // Command executed on the server
        // Only called from the client GO, on the corresponding GO on the server
        [Command]
        private void CmdMove(float x, float y) => RpcApplyForceToRigidBody(x, y);
        
        [Command] // Only called by clients
        private void CmdAttack(bool fireOneButton, bool fireTwoButton)
        {
            if (!fireOneButton && !fireTwoButton) return;
            if (weapon && weapon.CanAttack()) weapon.UseWeapon(fireOneButton, fireTwoButton);
        }

        [Command]
        private void CmdSwitchWeapon()
        {
            if (_weapons.Count == 0) return;
            SwitchWeapon(!weapon ? _weapons[0] : _weapons[(_weapons.IndexOf(weapon) + 1) % _weapons.Count]);
        }
        
        [ClientCallback]
        private void FixedUpdate()
        {
            // For physics
            if (!isLocalPlayer || MenuSettingsManager.Instance.isOpen) return;
            int horizontal = 0;
            int vertical = 0;
            if (InputManager.GetKeyPressed("Forward"))
                vertical++;
            if (InputManager.GetKeyPressed("Backward"))
                vertical--;
            if (InputManager.GetKeyPressed("Left"))
                horizontal--;
            if (InputManager.GetKeyPressed("Right"))
                horizontal++;
            CmdMove(horizontal, vertical);
        }

        [ClientCallback]
        private void Update()
        {
            // For inputs
            if (!isLocalPlayer) return;
            
            if (InputManager.GetKeyDown("OpenMenu"))
            {
                if (!MenuSettingsManager.Instance.isOpen)
                    MenuSettingsManager.Instance.OpenMenu();
                else
                    MenuSettingsManager.Instance.CloseMenu();
                return;
            }

            if (InputManager.GetKeyDown("OpenInventory"))
            {
                if (!_inventory.IsOpen)
                    _inventory.Open();
                else
                    _inventory.Close();
                return;
            }
            
            if (_inventory.IsOpen) return;

            CmdAttack(InputManager.GetKeyDown("DefaultAttack"), 
                InputManager.GetKeyDown("SpecialAttack"));

            // Change class (for testing)
            if (Input.GetKeyDown(KeyCode.C))
                CmdSwitchPlayerClass(playerClass == PlayerClasses.Archer ? PlayerClasses.Mage :
                    playerClass == PlayerClasses.Mage ? PlayerClasses.Warrior : PlayerClasses.Archer);

            if (netIdentity.isServer && Input.GetKeyDown(KeyCode.K))
            {
                NetworkServer.Spawn(LocalGameManager.Instance.weaponGenerator.GenerateBow().gameObject);
                Debug.Log("Spawned !");
            }

            if (netIdentity.isServer && Input.GetKeyDown(KeyCode.P))
            {
                NetworkManager.singleton.ServerChangeScene("Level1Creation");
                Debug.Log("Changed scene !");
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                CmdSwitchWeapon();
                Debug.Log("Changed weapon !");
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
