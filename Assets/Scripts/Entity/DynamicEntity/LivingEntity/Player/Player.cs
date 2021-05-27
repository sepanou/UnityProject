using System;
using DataBanks;
using Entity.Collectibles;
using Entity.DynamicEntity.Weapon;
using Mirror;
using UI_Audio;
using UI_Audio.LivingEntityUI;
using UnityEngine;

namespace Entity.DynamicEntity.LivingEntity.Player {
	public enum PlayerClasses: byte { Mage, Warrior, Archer }
	
	public class Player: LivingEntity {
		private const int MaxItemInInventory = 20;
		public static event LocalPlayerClassChanged OnLocalPlayerClassChange;
		public static event RemotePlayerClassChanged OnRemotePlayerClassChange;
		public delegate void LocalPlayerClassChanged(ClassData data);
		public delegate void RemotePlayerClassChanged(ClassData data);
		public event EnergyChanged OnEnergyChange;
		public delegate void EnergyChanged(float ratio);

		[Header("Player Fields")]
		[SerializeField] private PlayerClassData classData;
		[SerializeField] private GameObject toSpawn;

		[SyncVar(hook = nameof(SyncPlayerNameChanged))] public string playerName;
		private void SyncPlayerNameChanged(string o, string n) {
			if (!isLocalPlayer)
				entityUI.SetNameTagField(n);
		}
		
		[SyncVar(hook = nameof(SyncPlayerClassChanged))] public PlayerClasses playerClass;
		private void SyncPlayerClassChanged(PlayerClasses o, PlayerClasses n) => SwitchClass(n);
		
		[SyncVar(hook = nameof(SyncWeaponChanged))] [SerializeField] protected Weapon.Weapon weapon;
		private void SyncWeaponChanged(Weapon.Weapon o, Weapon.Weapon n) => PlayerInfoManager.UpdateCurrentWeapon(n);
		
		[SyncVar(hook = nameof(SyncMoneyChanged))] private int _kibrient;
		[SyncVar(hook = nameof(SyncMoneyChanged))] private int _orchid;
		private void SyncMoneyChanged(int o, int n) => PlayerInfoManager.UpdateMoneyAmount(this);
		
		[SyncVar(hook = nameof(SyncEnergyChanged))] private int _energy;
		private void SyncEnergyChanged(int o, int n) => OnEnergyChange?.Invoke(_energy / (float) maxEnergy);
		
		[SyncVar] [SerializeField] private int maxEnergy;

		private Camera _mainCamera;
		// serialization of Weapon objects, but it does for GameObject !
		[ShowInInspector]
		private readonly SyncList<Weapon.Weapon> _weapons = new SyncList<Weapon.Weapon>();
		[ShowInInspector]
		private readonly SyncList<Charm> _charms = new SyncList<Charm>();
		private Inventory _inventory;
		private ContainerInventory _containerInventory;
		private PlayerUI _playerUI;
		
		public int Kibrient => _kibrient;
		public int Orchid => _orchid;

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteInt32(_energy);
			writer.WriteInt32(maxEnergy);
			writer.WriteInt32(_kibrient);
			writer.WriteInt32(_orchid);
			writer.WriteByte((byte) playerClass);
			writer.WriteString(playerName);
			writer.WriteWeapon(weapon);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			int newEnergy = reader.ReadInt32();
			maxEnergy = reader.ReadInt32();
			int newKibrient = reader.ReadInt32();
			int newOrchid = reader.ReadInt32();
			PlayerClasses newPlayerClass = (PlayerClasses) reader.ReadByte();
			string newPlayerName = reader.ReadString();
			Weapon.Weapon newWeapon = reader.ReadWeapon();
			
			if (newEnergy != _energy) {
				SyncEnergyChanged(_energy, newEnergy);
				_energy = newEnergy;
			}

			if (newPlayerClass != playerClass) {
				SyncPlayerClassChanged(playerClass, newPlayerClass);
				playerClass = newPlayerClass;
			}

			if (newKibrient != _kibrient || newOrchid != _orchid) {
				SyncMoneyChanged(_kibrient, newKibrient);
				_kibrient = newKibrient;
				_orchid = newOrchid;
			}

			if (newPlayerName != playerName) {
				SyncPlayerNameChanged(playerName, newPlayerName);
				playerName = newPlayerName;
			}
			
			if (newWeapon == weapon) return;
			SyncWeaponChanged(weapon, newWeapon);
			weapon = newWeapon;
		}

		private void Start() {
			DontDestroyOnLoad(this);
			Instantiate();
			if (isServer)
				_energy = maxEnergy;
			if (!isLocalPlayer) {
				OnRemotePlayerClassChange += ChangeAnimator;
				_playerUI = (PlayerUI) entityUI;
				if (!_playerUI) return;
				_playerUI.SetNameTagField(playerName);
				OnEnergyChange += _playerUI.SetEnergyBarValue;
				SyncEnergyChanged(_energy, _energy);
			}
			else {
				OnLocalPlayerClassChange += ChangeAnimator;
				_inventory = Manager.inventoryManager.playerInventory;
				_mainCamera = Manager.SetMainCameraToPlayer(this);
				_weapons.Callback += OnWeaponsUpdated;
				_charms.Callback += OnCharmsUpdated;
				// Only health / energy UI for the other players
				entityUI.Destroy();
				Manager.LocalPlayer = this;
				PlayerInfoManager.UpdateMoneyAmount(this);
				OnEnergyChange += PlayerInfoManager.UpdatePlayerPower;
				OnHealthChange += PlayerInfoManager.UpdatePlayerHealth;
			}
			
			SwitchClass(playerClass);
		}

		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();
			CmdSetPseudo(Manager.startMenuManager.GetPseudoText());
		}

		// Can be executed by both client & server (Synced data analysis) -> double check
		public bool HasEnoughEnergy(int amount) => _energy >= amount;
		
		public bool HasEnoughKibrient(int amount) => _kibrient >= amount;

		public bool HasEnoughOrchid(int amount) => _orchid >= amount;
    
		public bool IsFullInventory() => _weapons.Count >= MaxItemInInventory;

		public void SetContainerInventory(ContainerInventory inventory) => _containerInventory = inventory;

		private void ChangeAnimator(ClassData data) {
			if (Animator) Animator.runtimeAnimatorController = data.animatorController;
			if (spriteRenderer) spriteRenderer.sprite = data.defaultSprite;
			playerClass = data.playerClass;
			if (_playerUI) _playerUI.SetEnergyBarColor(playerClass);
		}
		
		private void SwitchClass(PlayerClasses @class) {
			ClassData data = @class == PlayerClasses.Warrior ? classData.warrior :
				@class == PlayerClasses.Mage ? classData.mage : classData.archer;

			if (isLocalPlayer)
				OnLocalPlayerClassChange?.Invoke(data);
			else
				OnRemotePlayerClassChange?.Invoke(data);
		}

		private void OnWeaponsUpdated(SyncList<Weapon.Weapon>.Operation op, int itemIndex, Weapon.Weapon oldItem, Weapon.Weapon newItem) {
			if (!isLocalPlayer) return;
			
			switch (op) {
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
		
		private void OnCharmsUpdated(SyncList<Charm>.Operation op, int itemIndex, Charm oldItem, Charm newItem) {
			if (!isLocalPlayer) return;
			
			switch (op) {
				case SyncList<Charm>.Operation.OP_ADD:
					_inventory.TryAddItem(newItem);
					break;
				case SyncList<Charm>.Operation.OP_CLEAR:
					_inventory.ClearInventory();
					break;
				case SyncList<Charm>.Operation.OP_REMOVEAT:
					_inventory.TryRemoveItem(oldItem);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(op), op, null);
			}
		}
		
		[Server] public bool TryReduceKibrient(int amount) {
			if (!HasEnoughKibrient(amount))
				return false;
			_kibrient -= amount;
			return true;
		}

		[Server] public void AddCharm(Charm charm) => _charms.Add(charm);

		[Server] public bool TryRemoveCharm(Charm charm) => _charms.Remove(charm);

		[Server] public void ReduceEnergy(int amount) {
			if (amount == 0) return;
			_energy = Math.Max(_energy - amount, 0);
			OnEnergyChange?.Invoke(_energy / (float) maxEnergy);
		}

		[Server] private void SwitchWeapon(Weapon.Weapon newWeapon) {
			if (weapon) {
				weapon.UnEquip();
				weapon = null;
			}
			weapon = newWeapon;
			weapon.Equip(this);
		}

		[Server] public void Collect(uint entityNetId) {
			if (!NetworkIdentity.spawned.TryGetValue(entityNetId, out NetworkIdentity entityIdentity)) return;
			if (!entityIdentity.gameObject.TryGetComponent(out Entity collectible)) return;
			
			switch (collectible) {
				case Weapon.Weapon wp:
					wp.isGrounded = false;
					wp.netIdentity.AssignClientAuthority(netIdentity.connectionToClient);
					wp.holder = this;
					wp.DisableInteraction(this);
					wp.RpcDisableInteraction(this);
					Transform wpParent = transform;
					wp.transform.parent = wpParent;
					wp.RpcSetWeaponParent(wpParent);
					if (!weapon) SwitchWeapon(wp);
					_weapons.Add(wp);
					if (wp.TryGetComponent(out NetworkTransform netTransform)) {
						netTransform.clientAuthority = true;
						wp.TargetSetClientAuthority(wp.connectionToClient, true);
					}
					break;
				case Charm _:
					break;
				case Money _:
					break;
			}
		}
		
		[ClientRpc] protected override void RpcDying() {
			Debug.Log("Player " + playerName + " is dead !");
		}

		[Command] private void CmdSetPseudo(string pseudo) => playerName = pseudo;

		[Command] public void CmdSwitchPlayerClass(PlayerClasses @class) => playerClass = @class;

		[Command] private void CmdAttack(bool fireOneButton, bool fireTwoButton) {
			if (!fireOneButton && !fireTwoButton) return;
			if (weapon && weapon.CanAttack()) weapon.UseWeapon(fireOneButton, fireTwoButton);
		}

		[Command] private void CmdSwitchWeapon() {
			if (_weapons.Count == 0) return;
			SwitchWeapon(!weapon ? _weapons[0] : _weapons[(_weapons.IndexOf(weapon) + 1) % _weapons.Count]);
		}
		
		[ClientCallback] private void FixedUpdate() {
			// For physics
			if (!isLocalPlayer || MenuSettingsManager.Instance.isOpen || !NetworkClient.ready) return;
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

		[ClientCallback] private void Update() {
			// For inputs
			if (!isLocalPlayer || !NetworkClient.ready) return;
			
			if (InputManager.GetKeyDown("OpenMenu")) {
				if (!MenuSettingsManager.Instance.isOpen)
					MenuSettingsManager.Instance.OpenMenu();
				else
					MenuSettingsManager.Instance.CloseMenu();
				return;
			}

			if (_inventory.IsOpen && _containerInventory && _containerInventory.IsOpen) {
				if (Input.GetMouseButtonDown(0))
					_containerInventory.TryMoveHoveredSlotItem(_inventory);
				else if (InputManager.GetKeyDown("OpenInventory"))
					InventoryManager.CloseAllInventories();
				return;
			}
			
			if (InputManager.GetKeyDown("OpenInventory")) {
				if (!_inventory.IsOpen)
					_inventory.Open();
				else
					_inventory.Close();
				return;
			}

			CmdAttack(InputManager.GetKeyDown("DefaultAttack"), 
				InputManager.GetKeyDown("SpecialAttack"));
			
			if (Input.GetKeyDown(KeyCode.N)) {
				CmdSwitchWeapon();
				Debug.Log("Changed weapon !");
			}
			
			if (netIdentity.isServer && Input.GetKeyDown(KeyCode.K))
			{
				//NetworkServer.Spawn(LocalGameManager.Instance.weaponGenerator.GenerateSword().gameObject);
				GameObject obj = Instantiate(toSpawn, Vector3.zero, Quaternion.identity);
				NetworkServer.Spawn(obj);
				Debug.Log("Spawned !");
			}
			
			if (Input.GetKeyDown(KeyCode.B)) GetAttacked(1);
			
			if (netIdentity.isServer && Input.GetKeyDown(KeyCode.V)) {
				NetworkServer.Spawn(LocalGameManager.Instance.weaponGenerator.GenerateBow().gameObject);
			}
		}
	}
	
	public static class PlayerSerialization {
		public static void WritePlayer(this NetworkWriter writer, Player player) {
			writer.WriteBoolean(player);
			if (player && player.netIdentity)
				writer.WriteNetworkIdentity(player.netIdentity);
		}

		public static Player ReadPlayer(this NetworkReader reader) {
			if (!reader.ReadBoolean()) return null;
			NetworkIdentity identity = reader.ReadNetworkIdentity();
			return !identity ? null : identity.GetComponent<Player>();
		}
	}
}
