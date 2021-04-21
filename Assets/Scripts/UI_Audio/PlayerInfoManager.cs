using System;
using System.Collections;
using DataBanks;
using Entity.DynamicEntity.LivingEntity.Player;
using Entity.DynamicEntity.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI_Audio
{
    public class PlayerInfoManager : MonoBehaviour
    {
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
        [SerializeField] private Button warningCancelButton, warningContinueButton;
        private UnityAction _warningPrevCancel, _warningPrevContinue;

        [Header("Current Weapon & Money Fields")] 
        [SerializeField] private Image currentWeapon;
        [SerializeField] private TMP_Text currentWeaponName, kibrientCount, orchidCount;
        [SerializeField] private Image specialAttackIcon, defaultAttackIcon;
        [SerializeField] private Image specialAttackCooldown, defaultAttackCooldown;
        private Coroutine _specialAttackCooldown, _defaultAttackCooldown;

        [Header("Player Class Fields")] [SerializeField]
        private Image classIcon;
        [SerializeField] private Image powerBar, healthBar;

        public static PlayerInfoManager Instance;
        
        private void OnEnable()
        {
            if (!Instance)
                Instance = this;
            else
            {
                Destroy(this);
                return;
            }

            _infoBoxMaxX = infoBox.anchorMax.x;
            _infoBoxMinX = infoBox.anchorMin.x;
            _infoBoxGapX = _infoBoxMaxX - _infoBoxMinX;
            Player.OnLocalPlayerClassChange += ChangeLocalPlayerClassInfo;
            infoBox.gameObject.SetActive(false);
            dialogBox.gameObject.SetActive(false);
            warningBox.gameObject.SetActive(false);
        }

        private IEnumerator WriteDialog(string text, float delay = 0.02f)
        {
            dialogText.text = "";
            foreach (char chr in text)
            {
                dialogText.text += chr;
                yield return new WaitForSeconds(delay);
            }
        }
        
        // Dialog box
        
        public void PrintDialog(string text)
        {
            if (!dialogBox || !dialogText) return;
            if (_dialogWriter != null)
                StopCoroutine(_dialogWriter);
            _dialogWriter = StartCoroutine(WriteDialog(text));
        }

        public void CloseDialogBox() => dialogBox.gameObject.SetActive(false);
        public void OpenDialogBox() => dialogBox.gameObject.SetActive(true);
        
        // Info box

        private IEnumerator OpeningInfoBox(bool isClosing, float step = 0.01f, float delay = 0.03f)
        {
            float anchorMaxY = infoBox.anchorMax.y;
            float anchorMinY = infoBox.anchorMin.y;

            if (isClosing)
            {
                for (float x = infoBox.anchorMax.x; x > _infoBoxMinX; x -= step)
                {
                    infoBox.anchorMax = new Vector2(x, anchorMaxY);
                    infoBox.anchorMin = new Vector2(x - _infoBoxGapX, anchorMinY);
                    yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                for (float x = infoBox.anchorMax.x; x < _infoBoxMaxX; x += step)
                {
                    infoBox.anchorMax = new Vector2(x, anchorMaxY);
                    infoBox.anchorMin = new Vector2(x - _infoBoxGapX, anchorMinY);
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        private bool IsSafeInfoBox()
        {
            if (!infoBox) return false;
            if (_infoBoxOpening != null)
                StopCoroutine(_infoBoxOpening);
            return true;
        }
        public void OpenInfoBox()
        {
            if (!IsSafeInfoBox()) return;
            _infoBoxOpening = StartCoroutine(OpeningInfoBox(false));
        }
        
        public void CloseInfoBox()
        {
            if (!IsSafeInfoBox()) return;
            _infoBoxOpening = StartCoroutine(OpeningInfoBox(true));
        }
        
        public void SetInfoText(string text) => infoText.text = text;
        
        // Warning Box
        
        public void SetButtonActions(UnityAction cancelFunction, UnityAction continueFunction)
        {
            _warningPrevCancel = cancelFunction;
            _warningPrevContinue = continueFunction;
            warningCancelButton.onClick.AddListener(_warningPrevCancel);
            warningContinueButton.onClick.AddListener(_warningPrevContinue);
        }

        public void RemoveButtonActions()
        {
            if (_warningPrevCancel != null)
                warningCancelButton.onClick.RemoveListener(_warningPrevCancel);
            if (_warningPrevContinue != null)
                warningContinueButton.onClick.RemoveListener(_warningPrevContinue);
        }
        
        public void SetWarningText(string text) => warningText.text = text;

        public void OpenWarningBox() => warningBox.gameObject.SetActive(true);
        
        // Player Class information

        private void ChangeLocalPlayerClassInfo(ClassData data)
        {
            if (classIcon)
                classIcon.sprite = data.classIcon;
            if (powerBar)
                powerBar.sprite = data.powerBar;
            if (healthBar)
                healthBar.sprite = data.healthBar;
            if (specialAttackIcon)
                specialAttackIcon.sprite = data.specialAttackIcon;
            if (defaultAttackIcon)
                defaultAttackIcon.sprite = data.defaultAttackIcon;
        }

        private void IsSafeAttackCooldown(float duration, ref Coroutine coroutine, Image img)
        {
            if (!img) return;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(FadeCooldown(duration, img));
        }

        private IEnumerator FadeCooldown(float duration, Image img, float delay = 0.05f)
        {
            img.fillAmount = 1f;
            yield return null;
            
            for (; duration > 0; duration -= delay)
            {
                img.fillAmount = duration;
                yield return new WaitForSeconds(delay);
            }

            img.fillAmount = 0f;
        }

        public void StartSpecialAttackCooldown(float duration)
            => IsSafeAttackCooldown(duration, ref _specialAttackCooldown, specialAttackCooldown);
        
        public void StartDefaultAttackCooldown(float duration)
            => IsSafeAttackCooldown(duration, ref _defaultAttackCooldown, defaultAttackCooldown);

        public void SetKibrientCount(int count) => kibrientCount.text = count.ToString();

        public void SetOrchidCount(int count) => orchidCount.text = count.ToString();

        public void SetEquippedWeaponSprite(Sprite sprite) => currentWeapon.sprite = sprite;

        public void SetEquippedWeaponName(string wpName) => currentWeaponName.text = wpName;
    }
}
