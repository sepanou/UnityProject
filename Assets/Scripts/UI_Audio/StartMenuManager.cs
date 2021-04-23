using System.Collections;
using System.Text.RegularExpressions;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI_Audio
{
    public class StartMenuManager : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private RectTransform defaultFields;
        [SerializeField] private RectTransform gameModeFields;
        [SerializeField] private RectTransform multiPlayerFields;
        [SerializeField] private RectTransform pseudoFields;
        [SerializeField] private TMP_InputField pseudoInput;

        [Header("Server related GameObjects")]
        [SerializeField] private NetworkManager manager;
        [SerializeField] private TMP_InputField ipAddressField;

        [Header("Start Menu")]
        [SerializeField] private RectTransform worldParticles;
        [SerializeField] private RectTransform startMenuCanvas;

        private PlayerInfoManager _infoManager;

        public static StartMenuManager Instance;

        private const string RegexPatternIPAddress =
            @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$";

        private void OnEnable()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            CloseSubFields();
            _infoManager = PlayerInfoManager.Instance;
        }

        private void CloseSubFields()
        {
            gameModeFields.gameObject.SetActive(false);
            multiPlayerFields.gameObject.SetActive(false);
            pseudoFields.gameObject.SetActive(false);
        }

        private void EnterGame()
        {
            AudioDB.Instance.PlayMusic("HubMusic");
            worldParticles.gameObject.SetActive(false);
            defaultFields.gameObject.SetActive(true);
            CloseSubFields();
            startMenuCanvas.gameObject.SetActive(false);
        }

        private bool ValidateIPAddressInput(string input)
        {
            if (input.ToLower() == "localhost")
                return true;
            Regex regex = new Regex(RegexPatternIPAddress);
            return regex.IsMatch(input);
        }
        
        public void ValidatePseudo()
        {
            pseudoFields.gameObject.SetActive(false);
            
            if (pseudoInput.text.Length >= 4)
            {
                gameModeFields.gameObject.SetActive(true);
                return;
            }
            
            void Activate() => pseudoFields.gameObject.SetActive(true);
            _infoManager.SetWarningButtonActions(Activate, Activate);
            _infoManager.SetWarningText("Pseudo length must be greater or equal to four...");
            _infoManager.OpenWarningBox();
        }

        private IEnumerator ClientConnectionProcedure(RectTransform currentFields)
        {
            if (currentFields)
                currentFields.gameObject.SetActive(false);
            
            while (!NetworkClient.isConnected)
            {
                if (!NetworkClient.active)
                {
                    if (currentFields)
                    {
                        void Activate() => currentFields.gameObject.SetActive(true);
                        _infoManager.SetWarningButtonActions(Activate, Activate);
                    }
                    _infoManager.SetWarningText("Timed out after not receiving any message...\n" +
                                                    "Connection Failed!");
                    _infoManager.OpenWarningBox();
                    yield break;
                }
                yield return null;
            }

            EnterGame();
        }
        
        private IEnumerator ServerLaunchProcedure(RectTransform currentFields)
        {
            if (currentFields)
                currentFields.gameObject.SetActive(false);
            
            while (!NetworkServer.active)
                yield return null;
            
            EnterGame();
        }

        public void HostServerAndClient()
        {
            if (NetworkClient.active)
            {
                Debug.LogWarning("Already trying to connect to address" + manager.networkAddress + "...");
                return;
            }
            manager.StartHost();
            StopAllCoroutines();
            StartCoroutine(ServerLaunchProcedure(gameModeFields));
        }

        public void ConnectToServer()
        {
            if (NetworkClient.active)
            {
                Debug.LogWarning("Already trying to connect to address" + manager.networkAddress + "...");
                return;
            }
            if (ipAddressField && ValidateIPAddressInput(ipAddressField.text))
            {
                manager.StartClient();
                manager.networkAddress = ipAddressField.text;
                StopAllCoroutines();
                StartCoroutine(ClientConnectionProcedure(multiPlayerFields));
            }
            else
            {
                PlayerInfoManager.Instance.SetWarningText("Invalid IP address format!");
                PlayerInfoManager.Instance.OpenWarningBox();
            }
        }

        public void StopServerAndOrClient()
        {
            if (MenuSettingsManager.Instance.worldCamera)
                MenuSettingsManager.Instance.worldCamera.transform.parent = manager.transform;
            // Client + Server
            if (NetworkServer.active && NetworkClient.isConnected)
                manager.StopHost();
            // Client only
            else if (NetworkClient.isConnected)
                manager.StopClient();
            // Server only
            else if (NetworkServer.active)
                manager.StopServer();
        }
        
        public void QuitApplication()
        {
            StopServerAndOrClient();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
