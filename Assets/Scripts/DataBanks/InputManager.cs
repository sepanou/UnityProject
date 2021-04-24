using System;
using System.Collections.Generic;
using System.IO;
using Entity.DynamicEntity;
using Entity.DynamicEntity.LivingEntity.Player;
using UI_Audio;
using UnityEngine;

namespace DataBanks
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "DataBanks/InputManager", order = 3)]
    public class InputManager : ScriptableObject
    {
        [Serializable]
        private class InputEntry
        {
            public string actionName;
            public KeyCode keyEntry;
        }

        [SerializeField] private InputEntry[] entries;
        private bool _modified;
        private string _path;

        public bool Initialize()
        {
            NPC.InputManager = this;
            Player.InputManager = this;
            ModifyControls.InputManager = this;
            PlayerInfoManager.InputManager = this;
            Door.InputManager = this;

            _modified = false;
            _path = Path.Combine(Application.persistentDataPath, "InputManager.json");
            LoadData();
            return true;
        }

        private void LoadData()
        {
            try
            {
                string data = File.ReadAllText(_path);
                InputEntry[] temp = JsonSerializer.FromJsonArray<InputEntry>(data);
                if (temp.Length == 0)
                    SaveData();
                else
                    entries = temp;
            }
            catch (Exception)
            {
                SaveData();
            }
        }

        public bool GetKeyDown(string action)
        {
            foreach (InputEntry entry in entries)
            {
                if (entry.actionName != action) continue;
                return Input.GetKeyDown(entry.keyEntry);
            }
            Debug.LogWarning("The specified action name is unknown...");
            return false;
        }
        
        public bool GetKeyPressed(string action)
        {
            foreach (InputEntry entry in entries)
            {
                if (entry.actionName != action) continue;
                return Input.GetKey(entry.keyEntry);
            }
            Debug.LogWarning("The specified action name is unknown...");
            return false;
        }

        public bool TryGetKeyFromAction(string action, out KeyCode keyCode)
        {
            foreach (InputEntry entry in entries)
            {
                if (entry.actionName != action) continue;
                keyCode = entry.keyEntry;
                return true;
            }

            keyCode = KeyCode.None;
            return false;
        }

        public void ChangeKeyEntry(string action, KeyCode key)
        {
            foreach (InputEntry entry in entries)
            {
                if (entry.actionName != action) continue;
                _modified = entry.keyEntry != key;
                entry.keyEntry = key;
                return;
            }
            Debug.LogWarning("The specified action name is unknown...");
        }

        public void SaveData()
        {
            if (!_modified) return;
            _modified = false;
            string data = JsonSerializer.ToJsonArray(entries);
            File.WriteAllText(_path, data);
        }
    }
    
    [Serializable]
    public static class JsonSerializer
    {
        // Use to serialize unique fields represented by a List<T> or an array T[]
        public static T[] FromJsonArray<T>(string data)
        {
            Array<T> obj = JsonUtility.FromJson<Array<T>>(data);
            return obj.array;
        }

        public static string ToJsonArray<T>(T[] array, bool prettyPrint = false)
        {
            Array<T> obj = new Array<T> {array = array};
            return JsonUtility.ToJson(obj, prettyPrint);
        }
        
        public static List<T> FromJsonList<T>(string data)
        {
            ListContainer<T> obj = JsonUtility.FromJson<ListContainer<T>>(data);
            return obj.list;
        }

        public static string ToJsonList<T>(List<T> list, bool prettyPrint = false)
        {
            ListContainer<T> obj = new ListContainer<T> {list = list};
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        [Serializable]
        private class ListContainer<T>
        {
            public List<T> list;
        }
        
        [Serializable]
        private class Array<T>
        {
            public T[] array;
        }
    }
}
