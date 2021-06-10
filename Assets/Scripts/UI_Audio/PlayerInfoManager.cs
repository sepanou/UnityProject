﻿using System;
using System.Collections;
using DataBanks;
using Entity.Collectibles;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using Entity.DynamicEntity.Weapon.MeleeWeapon;
using Entity.DynamicEntity.Weapon.RangedWeapon;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.UI;

namespace UI_Audio {
	public class PlayerInfoManager: MonoBehaviour {
		[Header("Popup Boxes")]
		[SerializeField] private RectTransform infoBox;
		[SerializeField] private TMP_Text infoText;
		private Coroutine _infoBoxOpening;
		private float _infoBoxGapX, _infoBoxMaxX, _infoBoxMinX;
		[SerializeField] private RectTransform dialogBox;
		[SerializeField] private TMP_Text dialogText;
		private Coroutine _dialogWriter;
		[SerializeField] private RectTransform warningBox;
		[SerializeField] private TMP_Text warningText;
		[SerializeField] private Button warningButton;
		private UnityAction _warningButtonAction;

		[Header("Current Weapon & Money Fields")]
		[SerializeField] private RectTransform weaponMoneyCanvas;
		[SerializeField] private Image currentWeapon;
		[SerializeField] private TMP_Text currentWeaponName, kibrientCount, orchidCount;
		[SerializeField] private Image specialAttackIcon, defaultAttackIcon;
		[SerializeField] private Image specialAttackCooldown, defaultAttackCooldown;
		private Coroutine _specialAttackCooldown, _defaultAttackCooldown;
		
		[Header("Weapon Descriptions")]
		public RangedWeaponDescription rangedWeaponDescription;
		public MeleeWeaponDescription meleeWeaponDescription;
		public CharmDescription charmDescription;

		[Header("Player Class Fields")]
        [SerializeField] private RectTransform playerClassCanvas;
        [SerializeField] private Image classIcon, powerBar, healthBar;
        [SerializeField] public DisplayKey displayKey;
        
		[NonSerialized] public static LanguageManager LanguageManager;
		[NonSerialized] public static InputManager InputManager;
		[NonSerialized] public static PlayerInfoManager Instance;

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

		public void Initialize() {
			StartMenuManager.InfoManger = this;
			
			// Info box
			_infoBoxMaxX = infoBox.anchorMax.x;
			_infoBoxMinX = infoBox.anchorMin.x;
			_infoBoxGapX = _infoBoxMaxX - _infoBoxMinX;
			infoBox.gameObject.SetActive(false);
			CloseInfoBox();
			
			// Dialog box
			dialogBox.gameObject.SetActive(false);

			// Warning box
			warningBox.gameObject.SetActive(false);
						
			// Descriptive menus
			rangedWeaponDescription.gameObject.SetActive(false);
			meleeWeaponDescription.gameObject.SetActive(false);
			charmDescription.gameObject.SetActive(false);
			
			// Player Class UI
			Player.OnLocalPlayerClassChange += ChangeLocalPlayerClassInfo;
			UpdateCurrentWeapon(null);
			
			// Display Key
			displayKey.StopDisplay();
		}

		/// <summary>
		/// Writes a dialog
		/// </summary>
		/// <param name="dialogKeys">An array with the dialog part keys (see LanguageManager) to print, one after the other</param>
		/// <param name="callback">Method to call when the dialog is finished</param>
		/// <param name="sudden">Optional: set to true if there was already a dialog printed before BY THE SAME NPC</param>
		/// <param name="delay">Optional: the delay between each character printing</param>
		/// <returns></returns>
		private IEnumerator WriteDialog(string[] dialogKeys, UnityAction callback, bool sudden = false, float delay = 0.02f) {
			if (!LanguageManager)
				yield break;
			
			dialogText.text = "";
			
			if (!sudden)
				yield return new WaitUntil(() => InputManager.GetKeyUp("Interact"));
			
			dialogBox.gameObject.SetActive(true);

			foreach (string dialogKey in dialogKeys) {
				string toPrint = LanguageManager[dialogKey];
				
				foreach (char chr in toPrint) {
					dialogText.text += chr;
					yield return new WaitForSeconds(delay);
					
					if (!InputManager.GetKeyPressed("Interact")) continue;
					dialogText.text = toPrint;
					yield return new WaitForSeconds(delay);
					break;
				}

				yield return new WaitUntil(() => InputManager.GetKeyDown("Interact"));
			}
			
			while (!InputManager.GetKeyDown("Interact"))
				yield return null;
			
			dialogBox.gameObject.SetActive(false);
			callback?.Invoke();
		}
		
		// Dialog box
		public void PrintDialog(string[] dialogKeys, UnityAction callback, bool sudden = false) {
			if (!dialogBox || !dialogText) return;
			if (_dialogWriter != null)
				StopCoroutine(_dialogWriter);
			_dialogWriter = StartCoroutine(WriteDialog(dialogKeys, callback, sudden));
		}

		// Info box
		private IEnumerator OpeningInfoBox(bool isClosing, float step = 0.01f, float delay = 0.03f) {
			float anchorMaxY = infoBox.anchorMax.y;
			float anchorMinY = infoBox.anchorMin.y;

			if (isClosing) {
				for (float x = infoBox.anchorMax.x; x > _infoBoxMinX; x -= step) {
					infoBox.anchorMax = new Vector2(x, anchorMaxY);
					infoBox.anchorMin = new Vector2(x - _infoBoxGapX, anchorMinY);
					yield return new WaitForSeconds(delay);
				}
				infoBox.gameObject.SetActive(false);
			} else {
				infoBox.gameObject.SetActive(true);
				for (float x = infoBox.anchorMax.x; x < _infoBoxMaxX; x += step) {
					infoBox.anchorMax = new Vector2(x, anchorMaxY);
					infoBox.anchorMin = new Vector2(x - _infoBoxGapX, anchorMinY);
					yield return new WaitForSeconds(delay);
				}
			}
		}

		private bool IsSafeInfoBox() {
			if (!infoBox) return false;
			if (_infoBoxOpening != null)
				StopCoroutine(_infoBoxOpening);
			return true;
		}
		
		public void OpenInfoBox() {
			if (!IsSafeInfoBox()) return;
			_infoBoxOpening = StartCoroutine(OpeningInfoBox(false));
		}
		
		public void CloseInfoBox() {
			if (!IsSafeInfoBox()) return;
			_infoBoxOpening = StartCoroutine(OpeningInfoBox(true));
		}

		public IEnumerator DelayInfoBoxClosure(float delay) {
			yield return new WaitForSeconds(delay);
			CloseInfoBox();
		}
		
		public void SetInfoText(string text) => infoText.text = text;
		
		// Warning Box
		
		public void SetWarningButtonActions(UnityAction ackFunction) {
			RemoveWarningButtonActions();
			_warningButtonAction = ackFunction;
			warningButton.onClick.AddListener(_warningButtonAction);
		}

		// Called via inspector
		public void RemoveWarningButtonActions() {
			if (_warningButtonAction != null)
				warningButton.onClick.RemoveListener(_warningButtonAction);
		}
		
		public void SetWarningText(string text) => warningText.text = text;

		public void OpenWarningBox() => warningBox.gameObject.SetActive(true);
		
		// Player Class information
		private void ChangeLocalPlayerClassInfo(ClassData data) {
			if (classIcon)
				classIcon.sprite = data.classIcon;
			powerBar.sprite = data.powerBar;
			healthBar.sprite = data.healthBar;
			specialAttackIcon.sprite = data.specialAttackIcon;
			defaultAttackIcon.sprite = data.defaultAttackIcon;
		}

		private void IsSafeAttackCooldown(float duration, ref Coroutine coroutine, Image img) {
			if (!img) return;
			if (coroutine != null)
				StopCoroutine(coroutine);
			coroutine = StartCoroutine(FadeCooldown(duration, img));
		}

		private static IEnumerator FadeCooldown(float duration, Image img, float delay = 0.05f) {
			for (float remainingTime = duration; remainingTime > 0; remainingTime -= delay) {
				img.fillAmount = 1f - remainingTime / duration;
				yield return new WaitForSeconds(delay);
			}
			img.fillAmount = 1f;
		}
		
		public void UpdateCurrentWeapon(Weapon wp) {
			if (!wp) {
				currentWeapon.enabled = false;
				currentWeaponName.text = "";
				return;
			}

			currentWeapon.enabled = true;
			currentWeapon.sprite = wp.GetSpriteRenderer().sprite;
			currentWeaponName.text = wp.GetWeaponName();
		}

		public void UpdatePlayerHealth(float ratio) => healthBar.fillAmount = ratio;

		public void UpdatePlayerPower(float ratio) => powerBar.fillAmount = ratio;

		public void StartSpecialAttackCooldown(float duration)
			=> IsSafeAttackCooldown(duration, ref _specialAttackCooldown, specialAttackCooldown);
		
		public void StartDefaultAttackCooldown(float duration)
			=> IsSafeAttackCooldown(duration, ref _defaultAttackCooldown, defaultAttackCooldown);
		

		public void UpdateMoneyAmount(Player player) {
			kibrientCount.text = player.Kibrient.ToString();
			orchidCount.text = player.Orchid.ToString();
		}

		public void HidePlayerClassUI() {
			playerClassCanvas.gameObject.SetActive(false);
			weaponMoneyCanvas.gameObject.SetActive(false);
		}
		
		public void ShowPlayerClassUI() {
			playerClassCanvas.gameObject.SetActive(true);
			weaponMoneyCanvas.gameObject.SetActive(true);
		}

		// Weapon descriptive menus

		private void HideAllDescriptions() {
			meleeWeaponDescription.gameObject.SetActive(false);
			rangedWeaponDescription.gameObject.SetActive(false);
			charmDescription.gameObject.SetActive(false);
		}
		
		public RectTransform ShowRangedWeaponDescription(RangedWeaponData data) {
			HideAllDescriptions();
			rangedWeaponDescription.SetData(data);
			rangedWeaponDescription.gameObject.SetActive(true);
			return rangedWeaponDescription.rectTransform;
		}
		
		public RectTransform ShowMeleeWeaponDescription(MeleeWeaponData data) {
			HideAllDescriptions();
			meleeWeaponDescription.SetData(data);
			meleeWeaponDescription.gameObject.SetActive(true);
			return meleeWeaponDescription.rectTransform;
		}
		
		public RectTransform ShowCharmDescription(CharmData data) {
			HideAllDescriptions();
			charmDescription.SetData(data);
			charmDescription.gameObject.SetActive(true);
			return charmDescription.rectTransform;
		}
	}
}
