﻿using System.Collections;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Mirror;
using UnityEngine;

namespace UI_Audio
{ 
    public enum LocalGameStates
    {
        Start,
        InGame,
        Quit,
        None
    }
    
    public class LocalGameManager : MonoBehaviour
    {
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
        
        private LocalGameStates _localState;
        public LocalGameStates LocalState => _localState;

        public static LocalGameManager Instance;

        private void Awake()
        {
            Debug.Log("Hi");
            if (!Instance)
                Instance = this;
            else
            {
                Destroy(this);
                return;
            }

            // Find a way to define when it is server-only => no need to load UI stuff

            StartCoroutine(LoadGameDependencies(true));
        }

        private void Start()
        {
            _localState = LocalGameStates.None;
            SetLocalGameState(LocalGameStates.Start);
        }

        private IEnumerator LoadGameDependencies(bool loadUI = false)
        {
            Debug.Log("Loading dependencies...");
            yield return new WaitUntil(languageManager.Initialize);
            yield return new WaitUntil(audioManager.Initialize);
            yield return new WaitUntil(inputManager.Initialize);
            languageManager.InitLanguage();
            Debug.Log("Language...");
            if (!loadUI) yield break;
            yield return new WaitUntil(mouseCursor.Initialize);
            yield return new WaitUntil(menuSettingsManager.Initialize);
            yield return new WaitUntil(playerInfoManager.Initialize);
            yield return new WaitUntil(inventoryManager.Initialize);
            Debug.Log("End...");
        }

        public void SetLocalGameState(LocalGameStates state)
        {
            if (state == _localState) return;
            Debug.Log("Changed Local State to " + state);
            _localState = state;

            switch (_localState)
            {
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

        public Camera SetMainCameraToPlayer(Player player)
        {
            Transform camTransform = worldCamera.transform;
            worldCamera.gameObject.SetActive(true);
            camTransform.parent = player.transform;
            camTransform.localPosition = new Vector3(0, 0, -10);
            return worldCamera;
        }
    }
}
