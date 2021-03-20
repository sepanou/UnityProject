using DataBanks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSettingsManager : MonoBehaviour
{
    [SerializeField] private string[] resolutions;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundsSource;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private RectTransform defaultMenu;

    public static MenuSettingsManager Instance;

    public bool isOpen;
    public InputManager inputManager;
    private FullScreenMode _fullScreenMode;
    
    private void Start()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
        DontDestroyOnLoad(this);
        _fullScreenMode = FullScreenMode.Windowed;
    }

    public void OpenMenu()
    {
        isOpen = true;
        defaultMenu.gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        isOpen = false;
        defaultMenu.gameObject.SetActive(false);
    }
    
    public void SetEventSystemActive(bool state)
    {
        eventSystem.enabled = state;
    }

    public void ChangeMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    
    public void ChangeSoundsVolume(float volume)
    {
        soundsSource.volume = volume;
    }

    public void ChangeScreenResolution(int index)
    {
        if (index >= resolutions.Length) return;
        string[] fields = resolutions[index].Replace(" ", "").Split('x');
        if (fields.Length != 2) return;
        if (!int.TryParse(fields[0], out int width) || !int.TryParse(fields[1], out int height)) return;
        Screen.SetResolution(width, height, _fullScreenMode);
    }

    public void ChangeScreenMode(int index)
    {
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

    public void ChangeMouseSensitivityX(float sensitivity)
    {
        // TODO
    }
    
    public void ChangeMouseSensitivityY(float sensitivity)
    {
        // TODO
    }
}
