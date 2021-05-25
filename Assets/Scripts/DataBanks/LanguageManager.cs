using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI_Audio;
using UnityEngine;
using UnityEngine.UI;

namespace DataBanks {
	[CreateAssetMenu(fileName = "LanguageManager", menuName = "DataBanks/LanguageManager", order = 5)]
	public class LanguageManager: ScriptableObject {
		public static event LanguageChanged OnLanguageChange;
		private static string _path;

		public delegate void LanguageChanged();
		private readonly List<Language> _languages = new List<Language>();
		private string _currentLanguageKey;
		private Language _currentLanguage;
		private TextAsset _frenchLanguageFile;

		[SerializeField] private Language defaultEnglish;

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
			[TextArea] public string translation;
		}
		
		public void Initialize() {
			TextTranslator.LanguageManager = this;
			PlayerInfoManager.LanguageManager = this;

			_path = Path.Combine(Application.persistentDataPath, "Languages");
			
			// UK English is the default - hard coded - language
			_currentLanguageKey = "English-UK";
			_currentLanguage = defaultEnglish;
			_languages.Add(defaultEnglish);
			
			// French is necessarily supported
			_frenchLanguageFile = Resources.Load<TextAsset>("French");
			if (_frenchLanguageFile)
				_languages.Add(JsonUtility.FromJson<Language>(_frenchLanguageFile.text));
			
			LoadData();
		}

		public static void InitLanguage() => OnLanguageChange?.Invoke();

		/// <summary>
		/// Loads all the .json files inside the 'Languages' folder
		/// </summary>
		private void LoadData() {
			if (!Directory.Exists(_path) || !File.Exists(Path.Combine(_path, "English-UK.json"))) {
				Directory.CreateDirectory(_path);
				
				StreamWriter writer;
				if (_frenchLanguageFile && !File.Exists(Path.Combine(_path, "French.json"))) {
					writer = File.CreateText(Path.Combine(_path, "French.json"));
					writer.Write(_frenchLanguageFile.text);
					writer.Close();
				}
				writer = File.CreateText(Path.Combine(_path, "English-UK.json"));
				writer.Write(JsonUtility.ToJson(_currentLanguage, true));
				writer.Close();
				return;
			}
			
			DirectoryInfo directory = new DirectoryInfo(_path);
			foreach (FileInfo file in directory.EnumerateFiles("*.json")) {
				try {
					// No need to load this specific file (hard coded)
					if (file.Name == "English-UK.json")
						continue;
					Language language = JsonUtility.FromJson<Language>(File.ReadAllText(file.FullName));
					if (language is null)
						Debug.LogWarning($"Language file '{file.Name}' was not successfully deserialized...");
					else
						_languages.Add(language);
				}
				catch (Exception e) {
					Debug.LogWarning($"An error occured while loading a language file: {file.Name}!");
					Debug.LogWarning(e.Message);
				}
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

		/// <summary>
		/// Essentially dedicated to build the Dropdown list of available languages
		/// </summary>
		public List<Dropdown.OptionData> GetAllLanguages()
			=> _languages.Select(language => new Dropdown.OptionData(language.languageKey)).ToList();

		public string this[string fieldKey] => GetTranslation(fieldKey);
	}
}
