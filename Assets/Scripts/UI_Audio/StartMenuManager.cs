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

		private const string RegexPatternIPAddress =
			@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$";

		private void Awake() {
			if (!Instance)
				Instance = this;
			else throw new Exception("created two instances of StartMenuManager");
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

		private bool ValidateIPAddressInput(string input) {
			if (input.ToLower() == "localhost")
				return true;
			Regex regex = new Regex(RegexPatternIPAddress);
			return regex.IsMatch(input);
		}
		
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

		private IEnumerator ClientConnectionProcedure(RectTransform currentFields) {
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
		
		private IEnumerator ServerLaunchProcedure(RectTransform currentFields) {
			if (currentFields)
				currentFields.gameObject.SetActive(false);
			
			while (!NetworkServer.active)
				yield return null;
			
			LocalGameManager.Instance.SetLocalGameState(LocalGameStates.InGame);
		}
    
		public void HostServerAndClient() {
			if (NetworkClient.active) {
				Debug.LogWarning("Already trying to connect to address" + manager.networkAddress + "...");
				return;
			}
			manager.StartHost();
			StopAllCoroutines();
			StartCoroutine(ServerLaunchProcedure(gameModeFields));
		}

		public void ConnectToServer() {
			if (NetworkClient.active) {
				Debug.LogWarning("Already trying to connect to address" + manager.networkAddress + "...");
				return;
			}
			if (ipAddressField && ValidateIPAddressInput(ipAddressField.text)) {
				manager.StartClient();
				manager.networkAddress = ipAddressField.text;
				StopAllCoroutines();
				StartCoroutine(ClientConnectionProcedure(multiPlayerFields));
			} else {
				PlayerInfoManager.Instance.SetWarningText("Invalid IP address format!");
				PlayerInfoManager.Instance.OpenWarningBox();
			}
		}

		public void StopServerAndOrClient() {
			LocalGameManager.Instance.worldCamera.transform.parent = manager.transform;
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
