using System;
using DataBanks;
using Entity.Collectibles;
using Mirror;
using UI_Audio;
using UI_Audio.Inventories;
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

		[SyncVar(hook = nameof(SyncWeaponChanged))] private int _weaponId = -1;
		private void SyncWeaponChanged(int o, int n) {
			if (_weapons[o]) _weapons[o].SetSpriteRendererVisible(false);
			if (_weapons[n]) _weapons[n].SetSpriteRendererVisible(true);
			if (isLocalPlayer) PlayerInfoManager.UpdateCurrentWeapon(_weapons[n]);
		}
		
		[SyncVar(hook = nameof(SyncMoneyChanged))] private int _kibrient;
		[SyncVar(hook = nameof(SyncMoneyChanged))] private int _orchid;
		private void SyncMoneyChanged(int o, int n) {
			if (!isLocalPlayer) return;
			PlayerInfoManager.UpdateMoneyAmount(this);
		}
		
		private int _defaultMaxEnergy;
		[SyncVar(hook = nameof(SyncEnergyChanged))] [SerializeField] private int maxEnergy;
		[SyncVar(hook = nameof(SyncEnergyChanged))] private int _energy;
		private void SyncEnergyChanged(int o, int n) => OnEnergyChange?.Invoke(_energy / (float) maxEnergy);

		private CharmData _currentCharmBonus;
		private Camera _mainCamera;
		[ShowInInspector]
		private readonly CustomSyncList<Weapon.Weapon> _weapons = new CustomSyncList<Weapon.Weapon>();
		[ShowInInspector]
		private readonly CustomSyncList<Charm> _charms = new CustomSyncList<Charm>();
		private Inventory _inventory;
		private ContainerInventory _containerInventory;
		private SellerInventory _sellerInventory;
		private PlayerUI _playerUI;
		private Weapon.Weapon Weapon => _weapons[_weaponId]; 
		
		public int Kibrient {
			get => _kibrient;
			[Server] set => _kibrient = value;
		}

		public int Orchid {
			get => _orchid;
			[Server] set => _orchid = value;
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState) {
			base.OnSerialize(writer, initialState);
			writer.WriteInt32(_energy);
			writer.WriteInt32(maxEnergy);
			writer.WriteInt32(_kibrient);
			writer.WriteInt32(_orchid);
			writer.WriteByte((byte) playerClass);
			writer.WriteString(playerName);
			writer.WriteInt32(_weaponId);
			
			if (initialState) {
				_charms.OnSerializeAll(writer);
				_weapons.OnSerializeAll(writer);
			} else {
				_charms.OnSerializeDelta(writer);
				_weapons.OnSerializeDelta(writer);
			}
			
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState) {
			base.OnDeserialize(reader, initialState);
			int newEnergy = reader.ReadInt32();
			int newMaxEnergy = reader.ReadInt32();
			int newKibrient = reader.ReadInt32();
			int newOrchid = reader.ReadInt32();
			PlayerClasses newPlayerClass = (PlayerClasses) reader.ReadByte();
			string newPlayerName = reader.ReadString();
			int newWeaponId = reader.ReadInt32();
			
			if (initialState) {
				_charms.OnDeserializeAll(reader);
				_weapons.OnDeserializeAll(reader);
			} else {
				_charms.OnDeserializeDelta(reader);
				_weapons.OnDeserializeDelta(reader);
			}
			
			if (newEnergy != _energy || newMaxEnergy != maxEnergy) {
				maxEnergy = newMaxEnergy;
				SyncEnergyChanged(_energy, newEnergy);
				_energy = newEnergy;
			}

			if (newPlayerClass != playerClass) {
				SyncPlayerClassChanged(playerClass, newPlayerClass);
				playerClass = newPlayerClass;
			}

			if (newKibrient != _kibrient || newOrchid != _orchid) {
				_kibrient = newKibrient;
				_orchid = newOrchid;
				SyncMoneyChanged(_kibrient, newKibrient);
			}

			if (newPlayerName != playerName) {
				SyncPlayerNameChanged(playerName, newPlayerName);
				playerName = newPlayerName;
			}

			if (newWeaponId == _weaponId) return;
			SyncWeaponChanged(_weaponId, newWeaponId);
			_weaponId = newWeaponId;
		}

		private void Start() {
			DontDestroyOnLoad(this);
			Instantiate();

			if (isServer) {
				if (isLocalPlayer) _kibrient = 500;
				_defaultMaxEnergy = maxEnergy;
				_energy = maxEnergy;
				_charms.Callback += OnCharmsUpdatedServer;
			}
			
			if (isClient) _weapons.Callback += OnWeaponsUpdated;
			
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
				_inventory = InventoryManager.playerInventory;
				_mainCamera = Manager.SetMainCameraToPlayer(this);
				_charms.Callback += OnCharmsUpdatedClient;
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
		
		public bool HasEnoughKibrient(int amount) => Kibrient >= amount;

		public bool HasEnoughOrchid(int amount) => _orchid >= amount;

		public bool HasWeaponEquipped(Weapon.Weapon wp) => Weapon == wp;
    
		public bool IsFullInventory() => _weapons.Count + _charms.Count >= MaxItemInInventory;

		[Client] public void SetContainerInventory(ContainerInventory inventory)
			=> _containerInventory = inventory;
		
		[Client] public void SetSellerInventory(SellerInventory inventory)
			=> _sellerInventory = inventory;

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

		[Client]
		private void OnWeaponsUpdated(SyncList<uint>.Operation op, int index, Weapon.Weapon item) {
			if (op == SyncList<uint>.Operation.OP_ADD && item)
				item.transform.SetParent(transform, false);
			
			if (!isLocalPlayer) return;
			
			switch (op) {
				case SyncList<uint>.Operation.OP_ADD:
					item.transform.localPosition = item.defaultCoordsWhenLikedToPlayer;
					if (!Weapon) CmdSwitchWeapon(item);
					_inventory.TryAddItem(item);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					_inventory.ClearInventory();
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					_inventory.TryRemoveItem(item);
					break;
				default:
					Debug.LogWarning("An error happened while updating the sync weapons list (Client)");
					break;
			}
		}

		[Client]
		private void OnCharmsUpdatedClient(SyncList<uint>.Operation op, int index, Charm item) {
			if (op == SyncList<uint>.Operation.OP_ADD && item)
				item.transform.SetParent(transform, false);
			
			if (!isLocalPlayer) return;
			
			switch (op) {
				case SyncList<uint>.Operation.OP_ADD:
					item.transform.SetParent(transform, false);
					_inventory.TryAddItem(item);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					_inventory.ClearInventory();
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					_inventory.TryRemoveItem(item);
					break;
				default:
					Debug.LogWarning("An error happened while updating the sync charm list (Client)");
					break;
			}
		}
		
		[Server]
		private void OnCharmsUpdatedServer(SyncList<uint>.Operation op, int index, Charm item) {
			switch (op) {
				case SyncList<uint>.Operation.OP_ADD:
					_currentCharmBonus += item.bonuses;
					maxEnergy += item.bonuses.powerBonus;
					maxHealth += item.bonuses.healthBonus;
					Speed = DefaultSpeed * (1 + _currentCharmBonus.speedBonus);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					maxHealth = DefaultMaxHealth;
					maxEnergy = _defaultMaxEnergy;
					Speed = DefaultSpeed;
					_currentCharmBonus = null;
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					_currentCharmBonus -= item.bonuses;
					maxEnergy -= item.bonuses.powerBonus;
					maxHealth -= item.bonuses.healthBonus;
					Speed = DefaultSpeed * (1 + _currentCharmBonus.speedBonus);
					break;
				default:
					Debug.LogWarning("An error happened while updating the sync charm list (Server)");
					break;
			}
		}
		
		[Server] public bool TryReduceKibrient(int amount) {
			if (!HasEnoughKibrient(amount))
				return false;
			Kibrient -= amount;
			return true;
		}
		
		[Server] public bool RemoveCharm(Charm charm) => _charms.Remove(charm);
		[Server] public bool RemoveWeapon(Weapon.Weapon wp) {
			if (!_weapons.Remove(wp)) return false;
			if (HasWeaponEquipped(wp)) _weaponId = -1;
			return true;
		}

		[Server] public void ReduceEnergy(int amount) {
			if (amount == 0) return;
			_energy = Math.Max(_energy - amount, 0);
			OnEnergyChange?.Invoke(_energy / (float) maxEnergy);
		}
		
		[Server] private void SwitchWeapon(Weapon.Weapon newWeapon) {
			if (_weapons.Count == 0 || !_weapons.Contains(newWeapon)) return;
			_weaponId = _weapons.IndexOf(newWeapon);
		}

		[Server] public void CollectWeapon(Weapon.Weapon wp) {
			// Interactions + Authority
			wp.netIdentity.AssignClientAuthority(netIdentity.connectionToClient);
			wp.SetIsGrounded(false);
			wp.DisableInteraction(this);
			wp.RpcDisableInteraction(this);
			// Transform
			wp.transform.SetParent(transform, false);
			// Set owner
			wp.LinkToPlayer(this);
			// Target authority for synchronization of networkTransforms
			_weapons.Add(wp);
			if (!wp.TryGetComponent(out NetworkTransform netTransform)) return; // Should never happen
			netTransform.clientAuthority = true;
			wp.TargetSetClientAuthority(wp.connectionToClient, true);
		}

		[Server] public void CollectCharm(Charm charm) {
			// Interactions
			charm.DisableInteraction(this);
			charm.RpcDisableInteraction(this);
			charm.SetIsGrounded(false);
			// Transform
			charm.transform.SetParent(transform, false);
			_charms.Add(charm);
		}

		[Server] public void Collect(uint entityNetId) {
			if (!NetworkIdentity.spawned.TryGetValue(entityNetId, out NetworkIdentity entityIdentity)) return;
			if (!entityIdentity.gameObject.TryGetComponent(out Entity collectible)) return;

			if (IsFullInventory()) {
				TargetPrintInfoMessage(connectionToClient, "Your inventory is full");
				return;
			}

			switch (collectible) {
				case Money _:
					// TODO
					return;
				case Weapon.Weapon wp:
					CollectWeapon(wp);
					break;
				case Charm charm:
					CollectCharm(charm);
					break;
				default:
					Debug.LogWarning("Error, unknown collectible type...");
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
			if (Weapon && Weapon.CanAttack()) Weapon.UseWeapon(fireOneButton, fireTwoButton);
		}

		[Command] private void CmdSwitchWeapon(Weapon.Weapon wp) {
			if (!(wp is null) && wp) {
				SwitchWeapon(wp);
				return;
			}
			
			if (_weapons.Count == 0) return;
			SwitchWeapon(!Weapon ? _weapons[0] : _weapons[(_weaponId + 1) % _weapons.Count]);
		}

		[TargetRpc] public void TargetPrintWarning(NetworkConnection target, string message) {
			PlayerInfoManager.RemoveWarningButtonActions();
			PlayerInfoManager.SetWarningText(message);
			PlayerInfoManager.OpenWarningBox();
		}

		[TargetRpc] public void TargetPrintInfoMessage(NetworkConnection target, string message) {
			PlayerInfoManager.SetInfoText(message);
			PlayerInfoManager.OpenInfoBox();
			StartCoroutine(PlayerInfoManager.DelayInfoBoxClosure(5)); // Auto close the info box
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
				if (!Manager.menuSettingsManager.isOpen)
					Manager.menuSettingsManager.OpenMenu();
				else
					Manager.menuSettingsManager.CloseMenu();
				return;
			}

			if (_inventory.IsOpen && _containerInventory && _containerInventory.IsOpen) {
				if (Input.GetMouseButtonDown(0)) 
					_containerInventory.TryMoveHoveredSlotItem(_inventory);
				else if (InputManager.GetKeyDown("OpenInventory")) 
					InventoryManager.CloseShopKeeperInventory(_containerInventory);
				return;
			}

			if (_sellerInventory && _sellerInventory.IsOpen && InputManager.GetKeyDown("OpenInventory")) {
				InventoryManager.CloseShopKeeperInventory(_sellerInventory);
				return;
			}

			if (InputManager.GetKeyDown("OpenInventory")) {
				if (_inventory.IsOpen)
					_inventory.Close();
				else
					_inventory.Open();
				return;
			}

			CmdAttack(InputManager.GetKeyDown("DefaultAttack"), 
				InputManager.GetKeyDown("SpecialAttack"));
			
			if (Input.GetKeyDown(KeyCode.N))
				CmdSwitchWeapon(null);

			if (isServer && Input.GetKeyDown(KeyCode.K)) {
				NetworkServer.Spawn(WeaponGenerator.GenerateBow().gameObject);
			}
			
			if (Input.GetKeyDown(KeyCode.B)) GetAttacked(1);
			
			if (isServer && Input.GetKeyDown(KeyCode.V)) {
				NetworkServer.Spawn(WeaponGenerator.GenerateCharm().gameObject);
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
