using System;
using System.Collections;
using DataBanks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio
{
    public class ModifyControls : MonoBehaviour
    {
        [NonSerialized] public static InputManager InputManager;
        
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private Button button;
        [SerializeField] private string actionName;

        private void Start()
        {
            if (!InputManager || !InputManager.TryGetKeyFromAction(actionName, out KeyCode key)) return;
            if (keyLabel)
                keyLabel.text = key.ToString();
        }

        private IEnumerator Listener()
        {
            if (!MenuSettingsManager.Instance) yield break;
            MenuSettingsManager.Instance.SetEventSystemActive(false);
            yield return null;
        
            button.image.CrossFadeColor(button.colors.pressedColor, 0f, false, true);
        
            while (!Input.anyKeyDown) yield return null;
        
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (!Input.GetKey(keyCode)) continue;
                keyLabel.text = keyCode.ToString();
                InputManager.ChangeKeyEntry(actionName, keyCode);
                break;
            }

            button.image.CrossFadeColor(button.colors.normalColor, 0.1f, false, true);
            MenuSettingsManager.Instance.SetEventSystemActive(true);
        }

        public void OnClick()
        {
            StopAllCoroutines();
            StartCoroutine(Listener());
        }
    }
}
