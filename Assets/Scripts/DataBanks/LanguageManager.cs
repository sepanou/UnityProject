using System;
using System.Collections.Generic;
using System.Linq;
using UI_Audio;
using UnityEngine;
using UnityEngine.UI;

namespace DataBanks {
	[CreateAssetMenu(fileName = "LanguageManager", menuName = "DataBanks/LanguageManager", order = 5)]
	public class LanguageManager: ScriptableObject {
		public static event LanguageChanged OnLanguageChange;

		public delegate void LanguageChanged();
		private List<Language> _languages = new List<Language>();
		private string _currentLanguageKey;
		private Language _currentLanguage;

		[Serializable]
		private class Language {
			public string languageKey;
			public FieldEntry[] fields;

			public bool TryGetTranslation(string fieldKey, out string result) {
				foreach (FieldEntry field in fields) {
					if (field.fieldKey != fieldKey) continue;
					result = field.translation;
					return true;
				}
				result = "";
				return false;
			}
		}

		[Serializable]
		private class FieldEntry {
			public string fieldKey;
			public string translation;
		}
		
		public void Initialize() {
			TextTranslator.LanguageManager = this;
			PlayerInfoManager.LanguageManager = this;

			_currentLanguageKey = "English-UK";
			_currentLanguage = null;
			LoadData();
		}

		public static void InitLanguage() => OnLanguageChange?.Invoke();

		private void LoadData() {
			TextAsset jsonFile = Resources.Load<TextAsset>("LanguageManager");
			if (jsonFile == null) return;
			
			try {
				List<Language> temp =
					JsonSerializer.FromJsonList<Language>(jsonFile.text);
				if (temp.Count == 0)
					Debug.LogWarning("LanguageManager.json is empty or faulty!");
				else {
					_languages = temp;
					TryGetLanguage(_currentLanguageKey, out _currentLanguage);
				}
			} catch (Exception) {
				Debug.LogWarning("LanguageManager.json file does not exist!");
			}
		}

		private bool TryGetLanguage(string languageKey, out Language result) {
			foreach (Language language in _languages.Where(language => language.languageKey == languageKey)) {
				result = language;
				return true;
			}

			result = null;
			return false;
		}
		
		private string GetTranslation(string fieldKey) {
			if (_currentLanguage == null)
				return "";
			if (!_currentLanguage.TryGetTranslation(fieldKey, out string translation))
				Debug.LogWarning("The field " + fieldKey + " does not exist!");
			return translation;
		}

		public void ChangeLanguage(string languageKey) {
			if (languageKey == _currentLanguageKey || !TryGetLanguage(languageKey, out Language output))
				return;
			_currentLanguageKey = languageKey;
			_currentLanguage = output;
			OnLanguageChange?.Invoke();
		}

		public List<Dropdown.OptionData> GetAllLanguages()
			=> _languages.Select(language => new Dropdown.OptionData(language.languageKey)).ToList();

		public string this[string fieldKey] => GetTranslation(fieldKey);
	}
}
