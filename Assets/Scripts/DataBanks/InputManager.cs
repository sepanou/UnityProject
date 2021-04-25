using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI_Audio;
using UnityEngine;

namespace DataBanks {
	[CreateAssetMenu(fileName = "InputManager", menuName = "DataBanks/InputManager", order = 3)]
	public class InputManager: ScriptableObject {
		[Serializable]
		private class InputEntry {
			public InputEntry(string actionName, KeyCode keyEntry) {
				this.actionName = actionName;
				this.keyEntry = keyEntry;
			}
			public string actionName;
			public KeyCode keyEntry;
		}

		[SerializeField] private InputEntry[] entries;
		private Dictionary<string, KeyCode> _actionToKey = new Dictionary<string, KeyCode>();
		private bool _modified;
		private string _path;

		public void Initialize() {
			ModifyControls.InputManager = this;
			PlayerInfoManager.InputManager = this;
			Door.InputManager = this;

			_modified = false;
			_path = Path.Combine(Application.persistentDataPath, "InputManager.json");
			LoadData();
		}

		private void SetEntries(InputEntry[] temp) {
			entries = temp;
			foreach (InputEntry entry in entries)
				_actionToKey[entry.actionName] = entry.keyEntry;
		}

		private void UpdateEntries()
			=> entries = _actionToKey.Select(entry => new InputEntry(entry.Key, entry.Value)).ToArray();

		private void LoadData() {
			try {
				InputEntry[] temp = JsonSerializer.FromJsonArray<InputEntry>(File.ReadAllText(_path));
				if (temp.Length == 0)
					SaveData();
				else
					SetEntries(temp);
			} catch (Exception) {
				SaveData();
			}
		}

		public bool GetKeyDown(string action) {
			if (!_actionToKey.ContainsKey(action)) {
				Debug.LogWarning("The specified action name is unknown...");
				return false;
			}
			return Input.GetKeyDown(_actionToKey[action]);
		}
		
		public bool GetKeyPressed(string action) {
			if (!_actionToKey.ContainsKey(action)) {
				Debug.LogWarning("The specified action name is unknown...");
				return false;
			}
			return Input.GetKey(_actionToKey[action]);
		}

		public bool TryGetKeyFromAction(string action, out KeyCode keyCode)
			=> _actionToKey.TryGetValue(action, out keyCode);

		public void ChangeKeyEntry(string action, KeyCode key) {
			if (!_actionToKey.ContainsKey(action)) {
				Debug.LogWarning("The specified action name is unknown...");
				return;
			}
			if (_actionToKey[action] == key) return;
			_modified = true;
			_actionToKey[action] = key;
		}

		public void SaveData() {
			if (!_modified) return;
			_modified = false;
			UpdateEntries();
			string data = JsonSerializer.ToJsonArray(entries);
			File.WriteAllText(_path, data);
		}
	}
	
	[Serializable]
	public static class JsonSerializer {
		// Use to serialize unique fields represented by a List<T> or an array T[]
		public static T[] FromJsonArray<T>(string data)
			=> JsonUtility.FromJson<Array<T>>(data).array;

		public static string ToJsonArray<T>(T[] array, bool prettyPrint = false)
			=> JsonUtility.ToJson(new Array<T> {array = array}, prettyPrint);

		public static List<T> FromJsonList<T>(string data)
			=> JsonUtility.FromJson<ListContainer<T>>(data).list;

		public static string ToJsonList<T>(List<T> list, bool prettyPrint = false)
			=> JsonUtility.ToJson(new ListContainer<T> {list = list}, prettyPrint);

		[Serializable]
		private class ListContainer<T> {
			public List<T> list;
		}
		
		[Serializable]
		private class Array<T> {
			public T[] array;
		}
	}
}
