using System;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using UI_Audio;
using UI_Audio.Inventories;
using UnityEngine;

public enum LocalGameStates { Start, Hub, Forest, Quit, None }

public class LocalGameManager: MonoBehaviour {
	[Header("Forest Visibility")]
	[SerializeField] [Tooltip("The time is in seconds")] [Range(0.1f, 10f)]
	public float visibilityUpdateDelay;

	[Header("DataBanks")] public InputManager inputManager;
	public LanguageManager languageManager;
	public WeaponGeneratorDB weaponGenerator;
	public AudioDB audioManager;
	public InputTexturesDB inputTexturesDB;

	[Header("User Interface")] public StartMenuManager startMenuManager;
	public MenuSettingsManager menuSettingsManager;
	public PlayerInfoManager playerInfoManager;
	public MouseCursor mouseCursor;
	public InventoryManager inventoryManager;

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
			LocalState = LocalGameStates.Hub;
			worldCamera.transform.SetParent(transform);
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
		inputTexturesDB.SetEntries();
	}

	public void SetLocalGameState(LocalGameStates state) {
		if (state == LocalState) return;
		Debug.Log($"Changed Local State from {LocalState} to " + state);
		LocalState = state;

		switch (LocalState) {
			case LocalGameStates.Start:
				worldCamera.transform.SetParent(transform);
				startMenuManager.StopServerAndOrClient();
				inventoryManager.CloseAllInventories();
				inventoryManager.ClearAllInventories();
				AudioDB.PlayMusic("MainMenuMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.OpenStartMenu();
				playerInfoManager.Initialize();
				playerInfoManager.HidePlayerClassUI();
				break;
			case LocalGameStates.Hub:
				AudioDB.PlayMusic("HubMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.CloseStartMenu();
				playerInfoManager.ShowPlayerClassUI();
				break;
			case LocalGameStates.Forest:
				AudioDB.PlayMusic("ForestMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.CloseStartMenu();
				playerInfoManager.displayKey.StopDisplay();
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

public static class CameraExtension {
	public static bool IsObjectVisible(this Camera camera, Renderer renderer)
		=> GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
}
