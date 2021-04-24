using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using UI_Audio;
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
	public Camera overlayCamera;
	public Camera worldCamera; // = player's camera when he enters the game

	public LocalGameStates LocalState { get; private set; }

	public static LocalGameManager Instance;

	private void Awake() {
		if (Instance is null)
			Instance = this;
		else {
			Destroy(this);
			return;
		}

		// Find a way to define when it is server-only => no need to load UI stuff
		LoadGameDependencies(true);
	}

	private void Start() {
		LocalState = LocalGameStates.None;
		SetLocalGameState(LocalGameStates.Start);
	}

	private void LoadGameDependencies(bool loadUI = false) {
		languageManager.Initialize();
		audioManager.Initialize();
		inputManager.Initialize();
		languageManager.InitLanguage();
		if (!loadUI) return;
		mouseCursor.Initialize();
		menuSettingsManager.Initialize();
		playerInfoManager.Initialize();
		inventoryManager.Initialize();
	}

	public void SetLocalGameState(LocalGameStates state) {
		if (state == LocalState) return;
		Debug.Log("Changed Local State to " + state);
		LocalState = state;

		switch (LocalState) {
			case LocalGameStates.Start:
				audioManager.PlayMusic("MainMenuMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.OpenStartMenu();
				playerInfoManager.HidePlayerClassUI();
				break;
			case LocalGameStates.InGame:
				audioManager.PlayMusic("HubMusic");
				menuSettingsManager.CloseMenu();
				startMenuManager.CloseStartMenu();
				playerInfoManager.ShowPlayerClassUI();
				break;
			case LocalGameStates.Quit:
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