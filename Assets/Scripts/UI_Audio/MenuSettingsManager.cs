﻿using System;
using DataBanks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Audio
{
    public class MenuSettingsManager : MonoBehaviour
    {
        [Header("Graphics Settings")]
        [SerializeField] private string[] resolutions;
        [SerializeField] private Dropdown resolutionDropdown, screenModeDropdown;
        
        [Header("Sound Settings")]
        [SerializeField] private AudioDB audioDB;
        [SerializeField] private AudioMixer audioManager;
        [SerializeField] private Slider musicSlider, soundSlider;

        [Header("Input Settings")]
        [SerializeField] private InputManager inputManager;

        [Header("Others")]
        [SerializeField] private Dropdown languageDropdown;
        [SerializeField] private LanguageManager languageManager;
        public RangedWeaponDescription rangedWeaponDescription;
        public MeleeWeaponDescription meleeWeaponDescription;
        public CharmDescription charmDescription;

        [Header("Cameras")]
        [SerializeField] private Camera defaultCamera;

        [Header("Menus logic")]
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private RectTransform defaultMenu;
        [SerializeField] private RectTransform controlsMenu;

        public static MenuSettingsManager Instance;
        public static Camera CurrentCamera;

        public bool isOpen;
        private FullScreenMode _fullScreenMode;
        private bool _modified;

        private void Awake() => SwitchToUICamera();

        private void OnEnable()
        {
            if (!InputManager.Instance)
                inputManager.Awake();
            if (!AudioDB.Instance)
                audioDB.Awake();
            if (!LanguageManager.Instance)
                languageManager.Awake();
        
            if (!Instance)
                Instance = this;
            else
            {
                Destroy(this);
                return;
            }
            
            _fullScreenMode = FullScreenMode.Windowed;
            _modified = false;
            LoadSettings();
            controlsMenu.gameObject.SetActive(false);
            CloseMenu();
            
            // Descriptive Menus
            rangedWeaponDescription.gameObject.SetActive(false);
            meleeWeaponDescription.gameObject.SetActive(false);
            charmDescription.gameObject.SetActive(false);
            
            audioDB.PlayMusic("MainMenuMusic");
        }

        public void SwitchToCamera(Camera newCamera)
        {
            if (CurrentCamera)
                CurrentCamera.gameObject.SetActive(false);
            CurrentCamera = newCamera;
            newCamera.gameObject.SetActive(true);
        }

        public void SwitchToUICamera() => SwitchToCamera(defaultCamera);

        // Load a float setting and returns the current setting value
        private float TryLoadFloatSetting(string key, float defaultValue, Action<float> action)
        {
            float res = PlayerPrefs.GetFloat(key, float.NaN);
            if (float.IsNaN(res))
            {
                PlayerPrefs.SetFloat(key, defaultValue);
                res = defaultValue;
            }
            action(res);
            return res;
        }
        
        // Load a int setting and returns the current setting value
        private int TryLoadIntSetting(string key, int defaultValue, Action<int> action)
        {
            int res = PlayerPrefs.GetInt(key, -1);
            if (res == -1)
            {
                PlayerPrefs.SetInt(key, defaultValue);
                res = defaultValue;
            }
            action(res);
            return res;
        }

        private void LoadSettings()
        {
            // Graphics
            resolutionDropdown.value =
                TryLoadIntSetting("ResolutionIndex", resolutionDropdown.value, ChangeScreenResolution);
            screenModeDropdown.value =
                TryLoadIntSetting("ScreenModeIndex", screenModeDropdown.value, ChangeScreenMode);
            // Sound Settings
            musicSlider.value =
                TryLoadFloatSetting("MusicVolume", musicSlider.value, ChangeMusicVolume);
            soundSlider.value =
                TryLoadFloatSetting("SoundVolume", soundSlider.value, ChangeSoundsVolume);
            // Others
            BuildLanguagesDropdown();
            languageDropdown.value =
                TryLoadIntSetting("LanguageIndex", languageDropdown.value, ChangeLanguage);
        }

        private void SaveSettings()
        {
            if (!_modified) return;
            _modified = false;
            // Graphics
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
            PlayerPrefs.SetInt("ScreenModeIndex", screenModeDropdown.value);
            // Sound Settings
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
            PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
            // Others
            PlayerPrefs.SetInt("LanguageIndex", languageDropdown.value);
        }

        public void OpenMenu()
        {
            isOpen = true;
            defaultMenu.gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            isOpen = false;
            SaveSettings();
            defaultMenu.gameObject.SetActive(false);
        }

        public void SaveInputManager() => InputManager.Instance.SaveData();
    
        public void SetEventSystemActive(bool state)
        {
            eventSystem.enabled = state;
        }

        public void ChangeMusicVolume(float volume)
        {
            _modified = true;
            audioManager.SetFloat("MusicVolume", Mathf.Log(volume) * 20);
        }
    
        public void ChangeSoundsVolume(float volume)
        {
            _modified = true;
            audioManager.SetFloat("EffectsVolume", Mathf.Log(volume) * 20);
        }

        public void ChangeScreenResolution(int index)
        {
            _modified = true;
            if (index >= resolutions.Length) return;
            string[] fields = resolutions[index].Replace(" ", "").Split('x');
            if (fields.Length != 2) return;
            if (!int.TryParse(fields[0], out int width) || !int.TryParse(fields[1], out int height)) return;
            Screen.SetResolution(width, height, _fullScreenMode);
        }

        public void ChangeScreenMode(int index)
        {
            _modified = true;
            switch (index)
            {
                case 0:
                    _fullScreenMode = FullScreenMode.Windowed;
                    break;
                case 1:
                    _fullScreenMode = FullScreenMode.MaximizedWindow;
                    break;
                case 2:
                    _fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 3:
                    _fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
            }

            Screen.fullScreenMode = _fullScreenMode;
        }

        public void ChangeLanguage(int index)
        {
            if (!languageManager || !languageDropdown) return;
            _modified = true;
            languageManager.ChangeLanguage(languageDropdown.options[languageDropdown.value].text);
        }

        private void BuildLanguagesDropdown()
        {
            if (!languageManager || !languageDropdown)
                return;
            languageDropdown.options = languageManager.GetAllLanguages();
        }
    }
}
