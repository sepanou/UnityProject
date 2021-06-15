using System;
using System.Collections;
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
	[NonSerialized] public Player WorldCameraHolder;

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
				if (LocalPlayer) {
					LocalPlayer.SetSpriteRendererVisible(true);
					SetMainCameraToPlayer(LocalPlayer);
				}
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

	public Camera SetMainCameraToPlayer(Player player, bool smoothMove = false) {
		Transform camTransform = worldCamera.transform;
		Transform playerTransform = player.transform;
		worldCamera.gameObject.SetActive(true);
		camTransform.SetParent(playerTransform, smoothMove);
		
		if (smoothMove) {
			StopAllCoroutines();
			StartCoroutine(SmoothMove(camTransform, playerTransform, -10));
		} else camTransform.localPosition = new Vector3(0, 0, -10);

		WorldCameraHolder = player;
		return worldCamera;
	}

	private IEnumerator SmoothMove(Transform toMove, Transform toGo, int zAxis) {
		Vector2 position, destination;
		
		do {
			position = toMove.position;
			destination = toGo.position;
			toMove.position = Vector3.MoveTowards(position, destination, 0.25f) + Vector3.forward * zAxis;
			yield return null;
		} while (Vector2.Distance(position, destination) > 0.1f);
		
		toMove.localPosition = new Vector3(0, 0, zAxis);
	}
}

public static class CameraExtension {
	public static bool IsObjectVisible(this Camera camera, Renderer renderer)
		=> GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
}
