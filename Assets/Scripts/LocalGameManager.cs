using System;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using UI_Audio;
using UI_Audio.Inventories;
using UnityEngine;

public enum LocalGameStates { Start, InGame, Quit, None }

public class LocalGameManager: MonoBehaviour {
	[Header("DataBanks")] public InputManager inputManager;
	public LanguageManager languageManager;
	public WeaponGeneratorDB weaponGenerator;
	public AudioDB audioManager;

	[Header("User Interface")] public StartMenuManager startMenuManager;
	public MenuSettingsManager menuSettingsManager;
	public PlayerInfoManager playerInfoManager;
	public InventoryManager inventoryManager;
	public MouseCursor mouseCursor;

	[Header("Cameras")] public Camera mouseAndParticlesCamera;
	public Camera worldCamera; // = player's camera when he enters the game

	[NonSerialized] public Player LocalPlayer;

	public LocalGameStates LocalState { get; private set; }

	public static LocalGameManager Instance;

	private void Awake() {
		if (!Instance)
			Instance = this;
		else {
			// Duplicates
			Destroy(this);
			return;
		}
		LoadGameDependencies(!Application.isBatchMode);
		Entity.Entity.InitClass(Instance);
	}

	private void Start() {
		LocalState = LocalGameStates.None;
		if (!Application.isBatchMode)
			SetLocalGameState(LocalGameStates.Start);
		else {
			LocalState = LocalGameStates.InGame;
			worldCamera.transform.SetParent(transform);
			inventoryManager.CloseAllInventories();
			menuSettingsManager.CloseMenu();
			startMenuManager.CloseStartMenu();
			playerInfoManager.HidePlayerClassUI();
		}
	}

	private void LoadGameDependencies(bool loadUI = false) {
		languageManager.Initialize();
		playerInfoManager.Initialize();
		LanguageManager.InitLanguage();
		if (!loadUI) return;
		inputManager.Initialize();
		audioManager.Initialize();
		mouseCursor.Initialize();
		menuSettingsManager.Initialize();
		inventoryManager.Initialize();
	}

	public void SetLocalGameState(LocalGameStates state) {
		if (state == LocalState) return;
		Debug.Log("Changed Local State to " + state);
		LocalState = state;

		switch (LocalState) {
			case LocalGameStates.Start:
				worldCamera.transform.SetParent(transform);
				startMenuManager.StopServerAndOrClient();
				inventoryManager.CloseAllInventories();
				AudioDB.PlayMusic("MainMenuMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.OpenStartMenu();
				playerInfoManager.HidePlayerClassUI();
				break;
			case LocalGameStates.InGame:
				AudioDB.PlayMusic("HubMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.CloseStartMenu();
				playerInfoManager.ShowPlayerClassUI();
				break;
		}
	}

	public Camera SetMainCameraToPlayer(Player player) {
		Transform camTransform = worldCamera.transform;
		worldCamera.gameObject.SetActive(true);
		camTransform.parent = player.transform;
		camTransform.localPosition = new Vector3(0, 0, -10);
		return worldCamera;
	}
}