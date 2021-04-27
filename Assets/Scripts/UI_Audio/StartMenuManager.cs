using System;
using System.Collections;
using System.Text.RegularExpressions;
using Mirror;
using TMPro;
using UnityEngine;

namespace UI_Audio {
	public class StartMenuManager: MonoBehaviour {
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
		private bool _isOpen;
		public bool IsOpen() => _isOpen;
		
		[NonSerialized] public static StartMenuManager Instance;
		[NonSerialized] public static PlayerInfoManager InfoManger;

		private readonly Regex _regexIpv4Address = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$");
		private readonly Regex _regexIpv6Address = new Regex(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
		private readonly Regex _regexURL = new Regex(@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");

		private void Awake() {
			if (!Instance)
				Instance = this;
			else {
				// Duplicates
				Destroy(this);
				// ReSharper disable once RedundantJumpStatement
				return;
			}
		}
		
		private void CloseSubFields() {
			gameModeFields.gameObject.SetActive(false);
			multiPlayerFields.gameObject.SetActive(false);
			pseudoFields.gameObject.SetActive(false);
		}

		public void CloseStartMenu() {
			_isOpen = false;
			worldParticles.gameObject.SetActive(false);
			startMenuCanvas.gameObject.SetActive(false);
		}

		public void OpenStartMenu() {
			_isOpen = true;
			worldParticles.gameObject.SetActive(true);
			defaultFields.gameObject.SetActive(true);
			CloseSubFields();
			startMenuCanvas.gameObject.SetActive(true);
		}

		private bool ValidateIPAddressInput(string input)
			=> input == "localhost" || _regexIpv4Address.IsMatch(input) || _regexIpv6Address.IsMatch(input) || _regexURL.IsMatch(input);

		public void ValidatePseudo() {
			pseudoFields.gameObject.SetActive(false);
			
			if (pseudoInput.text.Length >= 4) {
				gameModeFields.gameObject.SetActive(true);
				return;
			}
			
			void Activate() => pseudoFields.gameObject.SetActive(true);
			InfoManger.SetWarningButtonActions(Activate);
			InfoManger.SetWarningText("Pseudo length must be greater or equal to four...");
			InfoManger.OpenWarningBox();
		}

		private static IEnumerator ClientConnectionProcedure(RectTransform currentFields) {
			if (currentFields)
				currentFields.gameObject.SetActive(false);
			
			while (!NetworkClient.isConnected) {
				if (!NetworkClient.active) {
					if (currentFields) {
						void Activate() => currentFields.gameObject.SetActive(true);
						InfoManger.SetWarningButtonActions(Activate);
					}
					InfoManger.SetWarningText("Timed out after not receiving any message...\n" +
											  "Connection Failed!");
					InfoManger.OpenWarningBox();
					yield break;
				}
				yield return null;
			}

			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.InGame);
		}
		
		private static IEnumerator ServerLaunchProcedure(RectTransform currentFields) {
			if (currentFields)
				currentFields.gameObject.SetActive(false);
			
			while (!NetworkServer.active)
				yield return null;
			
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.InGame);
		}
    
		public void HostServerAndClient() {
			if (NetworkServer.active || NetworkClient.active) {
				PlayerInfoManager.Instance.SetWarningText("Client or Server already started...");
				PlayerInfoManager.Instance.OpenWarningBox();
				return;
			}

			try {
				manager.StartHost();
				StopAllCoroutines();
				StartCoroutine(ServerLaunchProcedure(gameModeFields));
			} catch (Exception e) {
				StopServerAndOrClient();
				PlayerInfoManager.Instance.SetWarningText("Unable to launch the server...\n" +
				                                          "Are you sure a server is not already launched?");
				PlayerInfoManager.Instance.OpenWarningBox();
				Debug.LogWarning(e.Message);
			}
		}

		public void ConnectToServer() {
			if (NetworkClient.active) {
				Debug.LogWarning("Already trying to connect to address" + manager.networkAddress + "...");
				return;
			}

			if (!ipAddressField) {
				PlayerInfoManager.Instance.SetWarningText("No address field found!");
				PlayerInfoManager.Instance.OpenWarningBox();
				return;
			}

			string address = ipAddressField.text.ToLower().Replace(" ", "").Replace("\n", "");
			if (!ValidateIPAddressInput(address)) {
				PlayerInfoManager.Instance.SetWarningText("Invalid IP address format!");
				PlayerInfoManager.Instance.OpenWarningBox();
				return;
			}
			
			try {
				manager.StartClient();
				manager.networkAddress = address;
				StopAllCoroutines();
				StartCoroutine(ClientConnectionProcedure(multiPlayerFields));
			} catch (Exception e) {
				StopServerAndOrClient();
				PlayerInfoManager.Instance.SetWarningText("Unable to join the server...");
				PlayerInfoManager.Instance.OpenWarningBox();
				Debug.LogWarning(e.Message);
			}
		}

		public void StopServerAndOrClient() {
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
		
		public void QuitApplication() {
			StopServerAndOrClient();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}
