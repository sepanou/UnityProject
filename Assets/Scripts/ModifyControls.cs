using System;
using System.Collections;
using DataBanks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifyControls : MonoBehaviour
{
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private Button button;
    [SerializeField] private string actionName;

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
            MenuSettingsManager.Instance.inputManager.ChangeKeyEntry(actionName, keyCode);
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
