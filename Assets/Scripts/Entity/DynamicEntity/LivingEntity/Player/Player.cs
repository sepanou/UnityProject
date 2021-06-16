using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public const string CheatCodePseudo = "sepanou";
		private const int MaxItemInInventory = 20;
		private const int PassiveEnergyRegen = 2;
		public readonly CustomEvent<ClassData> OnPlayerClassChange = new CustomEvent<ClassData>();
		private readonly CustomEvent<float> _onEnergyChange = new CustomEvent<float>();

		[Header("Player Fields")]
		[SerializeField] private PlayerClassData classData;

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
		private void SyncEnergyChanged(int o, int n) => _onEnergyChange?.Invoke(_energy / (float) maxEnergy);

		private CharmData _currentCharmBonus;
		private Camera _mainCamera;
		[ShowInInspector]
		private readonly CustomSyncList<Weapon.Weapon> _weapons = new CustomSyncList<Weapon.Weapon>();
		[ShowInInspector]
		private readonly CustomSyncList<Charm> _charms = new CustomSyncList<Charm>();
		private Coroutine _passiveEnergyRegenCoroutine;
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
			[Server]
			set {
				_orchid = value;
				FileStorage.SavePlayerOrchid(playerName, value);
			} 
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
		
		public override void OnStartClient() {
			base.OnStartClient();
			
			if (isLocalPlayer) return;
			if (!isServer) Instantiate();
			
			_weapons.Callback.AddListener(OnWeaponsUpdated);
			OnPlayerClassChange.AddListener(ChangeAnimator);
			_playerUI = (PlayerUI) entityUI;
			if (!_playerUI) return;
			_playerUI.SetNameTagField(playerName);
			_onEnergyChange.AddListener(_playerUI.SetEnergyBarValue);
			SyncEnergyChanged(_energy, _energy);
			SwitchClass(playerClass);
		}
		
		public override void OnStartLocalPlayer() {
			base.OnStartLocalPlayer();
			
			if (!isServer) Instantiate();
			
			_weapons.Callback.AddListener(OnWeaponsUpdated);
			OnPlayerClassChange.AddListener(ChangeAnimator);
			OnPlayerClassChange.AddListener(PlayerInfoManager.ChangeLocalPlayerClassInfo);
			SwitchClass(playerClass);
			
			CmdSetPseudo(Manager.startMenuManager.GetPseudoText());
			_inventory = InventoryManager.playerInventory;
			_mainCamera = Manager.SetMainCameraToPlayer(this);
			_charms.Callback.AddListener(OnCharmsUpdatedClient);
			// Only health / energy UI for the other players
			entityUI.Destroy();
			Manager.LocalPlayer = this;
			PlayerInfoManager.UpdateMoneyAmount(this);
			_onEnergyChange.AddListener(PlayerInfoManager.UpdatePlayerPower);
			OnHealthChange.AddListener(PlayerInfoManager.UpdatePlayerHealth);
		}

		public override void OnStopClient() {
			if (!isLocalPlayer && Manager.WorldCameraHolder == this) {
				Manager.worldCamera.transform.parent = transform.parent;
				Manager.WorldCameraHolder = null;
			}
			base.OnStopClient();
		}

		public override void OnStartServer() {
			base.OnStartServer();
			
			if (Manager.LocalState != LocalGameStates.Hub) {
				connectionToClient.Disconnect();
				return;
			}
			
			Instantiate();
			OnEntityDie.AddListener(SpectatorPlayer);
			OnPlayerClassChange.AddListener(ChangeAnimator);
			SwitchClass(playerClass);
			_defaultMaxEnergy = maxEnergy;
			_energy = maxEnergy;
			_charms.Callback.AddListener(OnCharmsUpdatedServer);
		}

		private new void Instantiate() {
			DontDestroyOnLoad(this);
			base.Instantiate();
		}

		[Server] public void ResetPlayer() {
			Health = DefaultMaxHealth;
			_energy = _defaultMaxEnergy;
			_charms.Clear();
			_weapons.Clear();
			_weaponId = -1;
			_currentCharmBonus = null;
			Kibrient = 0;
			Manager.SetMainCameraToPlayer(this);
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

		[Server] private IEnumerator PassiveEnergyRegeneration() {
			while (_energy + PassiveEnergyRegen < maxEnergy) {
				yield return new WaitForSeconds(1f);
				_energy += PassiveEnergyRegen;
			}

			_energy = maxEnergy;
			_passiveEnergyRegenCoroutine = null;
		}
		
		private void ChangeAnimator(ClassData data) {
			if (Animator) Animator.runtimeAnimatorController = data.animatorController;
			if (spriteRenderer) spriteRenderer.sprite = data.defaultSprite;
			playerClass = data.playerClass;
			if (_playerUI) _playerUI.SetEnergyBarColor(playerClass);
		}
		
		private void SwitchClass(PlayerClasses @class) {
			ClassData data = @class == PlayerClasses.Warrior ? classData.warrior :
				@class == PlayerClasses.Mage ? classData.mage : classData.archer;
			OnPlayerClassChange?.Invoke(data);
		}
		
		public int ApplyDamageBonuses(int damage, bool isSpecialAttack) {
			if (_currentCharmBonus is null) return damage;
			return (int) (damage * (isSpecialAttack
			? _currentCharmBonus
				.specialAttackDamageBonus
			: _currentCharmBonus
				.defaultAttackDamageBonus));
		}

		public float ApplyCooldownBonuses(float initialCooldown) {
			if (_currentCharmBonus is null) return initialCooldown;
			if (_currentCharmBonus.cooldownReduction >= 1) return 0f;
			return initialCooldown * (1 - _currentCharmBonus.cooldownReduction);
		}

		public bool IsSpriteVisible(SpriteRenderer sprite) => _mainCamera.IsObjectVisible(sprite);

		[Client]
		private void OnWeaponsUpdated(SyncList<uint>.Operation op, int index, Weapon.Weapon item) {
			switch (op) {
				case SyncList<uint>.Operation.OP_ADD:
					item.transform.SetParent(transform, false);
					item.SetSpriteRendererVisible(false);
					if (!isLocalPlayer) return;
					item.transform.localPosition = item.defaultCoordsWhenLikedToPlayer;
					if (!Weapon) CmdSwitchWeapon(item);
					_inventory.TryAddItem(item);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					if (!isLocalPlayer) return;
					_inventory.ClearInventory();
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					item.transform.SetParent(null, false);
					item.SetSpriteRendererVisible(true);
					if (!isLocalPlayer) return;
					_inventory.TryRemoveItem(item);
					break;
				default:
					Debug.LogWarning("An error happened while updating the sync weapons list (Client)");
					break;
			}
		}

		[Client]
		private void OnCharmsUpdatedClient(SyncList<uint>.Operation op, int index, Charm item) {
			switch (op) {
				case SyncList<uint>.Operation.OP_ADD:
					item.transform.SetParent(transform, false);
					item.SetSpriteRendererVisible(false);
					if (!isLocalPlayer) return;
					_inventory.TryAddItem(item);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					if (!isLocalPlayer) return;
					_inventory.ClearInventory();
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					item.SetSpriteRendererVisible(true);
					item.transform.SetParent(null, false);
					if (!isLocalPlayer) return;
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
					_energy += item.bonuses.powerBonus;
					maxHealth += item.bonuses.healthBonus;
					Health += item.bonuses.healthBonus;
					Speed = DefaultSpeed * (1 + _currentCharmBonus.speedBonus);
					break;
				case SyncList<uint>.Operation.OP_CLEAR:
					maxHealth = DefaultMaxHealth;
					Health = DefaultMaxHealth;
					maxEnergy = _defaultMaxEnergy;
					_energy = _defaultMaxEnergy;
					Speed = DefaultSpeed;
					_currentCharmBonus = null;
					break;
				case SyncList<uint>.Operation.OP_REMOVEAT:
					int changedHealth = Health - item.bonuses.healthBonus;
					_currentCharmBonus -= item.bonuses;
					maxEnergy -= item.bonuses.powerBonus;
					ReduceEnergy(item.bonuses.powerBonus);
					maxHealth -= item.bonuses.healthBonus;
					Health = changedHealth <= 0 ? Health : changedHealth;
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
			bool hadWeaponEquipped = HasWeaponEquipped(wp);
			if (!_weapons.Remove(wp)) return false;
			if (hadWeaponEquipped) _weaponId = -1;
			return true;
		}

		[Server] public void ReduceEnergy(int amount) {
			if (amount == 0) return;
			_energy = Math.Max(_energy - amount, 0);
			_onEnergyChange?.Invoke(_energy / (float) maxEnergy);
			if (_passiveEnergyRegenCoroutine is null)
				_passiveEnergyRegenCoroutine = StartCoroutine(PassiveEnergyRegeneration());
		}
		
		[Server] private void SwitchWeapon(Weapon.Weapon newWeapon) {
			if (_weapons.Count == 0 || !_weapons.Contains(newWeapon)) return;
			_weaponId = _weapons.IndexOf(newWeapon);
		}

		[Server] public void CollectWeapon(Weapon.Weapon wp) {
			wp.LinkToPlayer(this);
			_weapons.Add(wp);
		}

		[Server] public void CollectCharm(Charm charm) {
			charm.LinkToPlayer(this);
			_charms.Add(charm);
		}

		[Server] public void Collect(uint entityNetId) {
			if (!NetworkIdentity.spawned.TryGetValue(entityNetId, out NetworkIdentity entityIdentity)) return;
			if (!entityIdentity.gameObject.TryGetComponent(out Entity collectible)) return;

			if (collectible is Kibry kibry) {
				Kibrient += kibry.amount;
				NetworkServer.Destroy(kibry.gameObject);
				return;
			}
			
			if (IsFullInventory()) {
				if (collectible is Weapon.Weapon wp) wp.SetPlayerFound(false);
				TargetPrintInfoMessage(connectionToClient, "Your inventory is full");
				return;
			}

			switch (collectible) {
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
		
		private IEnumerator DeathAnimation() {
			Animator.SetTrigger(IsDeadId);
			yield return new WaitForSeconds(0.3f);
			SetSpriteRendererVisible(false);
		}

		[Server] private void SpectatorPlayer(LivingEntity living) {
			_weaponId = -1;
			Health = 0;
			Position = new Vector2(1000, 1000);
		}

		[ClientRpc] protected override void RpcDying() {
			Debug.Log("Player " + playerName + " is dead !");
			Animator.SetTrigger(IsDeadId);
			if (!isLocalPlayer) return;
			InventoryManager.CloseAllInventories();
			SpectateNextPlayer();
			PlayerInfoManager.SetInfoText(LanguageManager["#switch-view"]);
			PlayerInfoManager.OpenInfoBox();
		}

		private void SpectateNextPlayer() {
			List<Player> alivePlayers = FindObjectsOfType<Player>().ToList().FindAll(p => p.IsAlive);
			Player lastSpectating = Manager.WorldCameraHolder;
			Player toLocate = alivePlayers.Find(player => player != this && player != lastSpectating);
			if (!toLocate) return;
			Manager.SetMainCameraToPlayer(toLocate);
			PlayerInfoManager.SetInfoText(LanguageManager["#teleport-to"] + toLocate.playerName + ".");
			PlayerInfoManager.OpenInfoBox();
			StartCoroutine(PlayerInfoManager.DelayInfoBoxClosure(5));
		}

		[Command] private void CmdSetPseudo(string pseudo) {
			playerName = pseudo;
			_orchid = FileStorage.GetPlayerOrchid(pseudo);
		}

		[Command] public void CmdSwitchPlayerClass(PlayerClasses @class) {
			playerClass = @class;
			SyncPlayerClassChanged(playerClass, playerClass);
		}

		[Command] private void CmdAttack(bool defaultAttack, bool specialAttack) {
			if (!defaultAttack && !specialAttack) return;
			if (Weapon && Weapon.CanAttack()) Weapon.UseWeapon(defaultAttack, specialAttack);
		}

		[Command] private void CmdSwitchWeapon(Weapon.Weapon wp) {
			if (!(wp is null) && wp) {
				SwitchWeapon(wp);
				return;
			}
			
			if (_weapons.Count == 0) return;
			SwitchWeapon(!Weapon ? _weapons[0] : _weapons[(_weaponId + 1) % _weapons.Count]);
		}

		[Command] private void CmdDropItem(IInventoryItem item) => item?.Drop(this);

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
		
		[TargetRpc] public void TargetPrintDialog(NetworkConnection target, string[] dialogKey) {
			PlayerInfoManager.PrintDialog(dialogKey, null, true);
		}

		[Command] private void CmdCheatCode(string code) {
			if (playerName != CheatCodePseudo) return;
			GameObject obj = null;
			
			switch (code) {
				case "Bow":
					obj = WeaponGenerator.GenerateBow().gameObject;
					break;
				case "Sword":
					obj = WeaponGenerator.GenerateSword().gameObject;
					break;
				case "Staff":
					obj = WeaponGenerator.GenerateStaff().gameObject;
					break;
				case "Charm":
					obj = WeaponGenerator.GenerateCharm().gameObject;
					break;
			}
			
			if (obj is null) return;
			obj.transform.position = transform.position;
			NetworkServer.Spawn(obj);
		}


		[ClientCallback] private void FixedUpdate() {
			// For physics
			if (!isLocalPlayer || MenuSettingsManager.Instance.isOpen || !NetworkClient.ready)
				return;
			
			if (!IsAlive) return;

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
			if (!isLocalPlayer || !NetworkClient.ready)
				return;
			
			if (InputManager.GetKeyDown("OpenMenu")) {
				if (!Manager.menuSettingsManager.isOpen)
					Manager.menuSettingsManager.OpenMenu();
				else
					Manager.menuSettingsManager.CloseMenu();
				return;
			}
			
			if (!IsAlive) {
				if (InputManager.GetKeyDown("SwitchWeapon"))
					SpectateNextPlayer();
				return;
			}

			// Right click in inventory == Drop the item
			if (_inventory.IsOpen && Input.GetMouseButtonDown(1)) {
				InventorySlot lastHovered = InventorySlot.LastHovered;
				IInventoryItem item;
				if (lastHovered && (item = lastHovered.GetSlotItem()) != null && _inventory.Contains(item))
					CmdDropItem(item);
				return;
			}

			// Left click in inventory while trading with a NPC == Move the item to the other inventory
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

			if ((_sellerInventory && _sellerInventory.IsOpen)
			    || (_containerInventory && _containerInventory.IsOpen)
			    || _inventory.IsOpen)
				return;

			if (InputManager.GetKeyDown("SwitchWeapon")) {
				CmdSwitchWeapon(null);
				return;
			}
			
			if (!_inventory.IsOpen) {
				bool defaultAttack = InputManager.GetKeyDown("DefaultAttack");
				bool specialAttack = InputManager.GetKeyDown("SpecialAttack");
				if (defaultAttack || specialAttack)
					CmdAttack(defaultAttack,specialAttack);
			}

			if (playerName != CheatCodePseudo) return;
			
			if (Input.GetKeyDown(KeyCode.B)) CmdCheatCode("Bow");
			if (Input.GetKeyDown(KeyCode.K)) CmdCheatCode("Sword");
			if (Input.GetKeyDown(KeyCode.L)) CmdCheatCode("Staff");
			if (Input.GetKeyDown(KeyCode.V)) CmdCheatCode("Charm");
			if (Input.GetKeyDown(KeyCode.O)) CmdCheatCode("Projectile");
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
