using System;
using DataBanks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio {
	public class TextTranslator: MonoBehaviour {
		[SerializeField] private string fieldKey;
		/// Use '%s' to indicate where the word will be located
		[SerializeField] private string formatText;
		[SerializeField] private bool upperCase, lowerCase;
		private Text _text;
		private TMP_Text _tmpText;

		[NonSerialized] public static LanguageManager LanguageManager;

		private void Awake() {
			LanguageManager.OnLanguageChange += OnLanguageChange;
			TryGetComponent(out _tmpText);
			TryGetComponent(out _text);
		}

		private string Format(string word) => formatText.Replace("%s", word);

		private void OnLanguageChange() {
			if (!_text && !_tmpText)
				return;
			
			string translation = LanguageManager[fieldKey];
			
			if (lowerCase)
				translation = translation.ToLower();
			else if (upperCase)
				translation = translation.ToUpper();
			
			if  (formatText != "")
				translation = Format(translation);
			
			if (_text)
				_text.text = translation;
			if (_tmpText)
				_tmpText.text = translation;
		}
	}
}
