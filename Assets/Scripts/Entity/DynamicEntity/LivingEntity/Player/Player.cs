using System.Collections.Generic;
using DataBanks;
using Entity.Collectibles;
using Mirror;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Player
{
    public enum PlayerClasses
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
        
        [SerializeField] protected Weapon.Weapon weapon; // RpcSync
        [SyncVar] [SerializeField] private int energy;
        
        [SyncVar] public string playerName;
        [SyncVar] public PlayerClasses playerClass; // enum = serializable
        [SyncVar] private int _money = 0;
        
        private List<Charm> _charms; // Could use targetRpc -> no need for others to see our charms !
        // Should use SyncList of gameObjects (weapons) -> weaver does not support
        // serialization of Weapon objects, but it does for GameObject !
        private List<Weapon.Weapon> _weapons;
        private int _lastAnimationStateIndex;

        private void Start()
        {
            InstantiateLivingEntity();
            _weapons = new List<Weapon.Weapon>();
            _charms = new List<Charm>();
            _lastAnimationStateIndex = 0;
            SwitchPlayerClass(playerClass);
            SwitchPlayerClass(playerClass);
            mainCamera.gameObject.SetActive(isLocalPlayer);
        }

        // Can be executed by both client & server (Synced data analysis) -> double check
        public bool HasEnoughEnergy(int amount) => energy >= amount;
        public bool HasEnoughMoney(int amount) => _money >= amount;

        public Vector3 WorldToScreenPoint(Vector3 position)
            => mainCamera ? mainCamera.WorldToScreenPoint(position) : Vector3.zero;
        
        [ServerCallback]
        public void ReduceEnergy(int amount) => energy = amount >= energy ? 0 : energy - amount;

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
            if (weapon) weapon.RpcUnEquip();
            weapon = newWeapon;
            weapon.RpcEquip(this);
        }

        [ServerCallback]
        private void CollectWeapon(Weapon.Weapon wp)
        {
            wp.holder = this;
            wp.RpcSetWeaponParent(transform);
            if (weapon)
                wp.RpcUnEquip();
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
}
