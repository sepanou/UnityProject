using System;
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

            public InputEntry(string actionName, KeyCode keyEntry)
            {
                this.actionName = actionName;
                this.keyEntry = keyEntry;
            }
        }

        [SerializeField] private InputEntry[] entries;

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

        public void ChangeKeyEntry(string action, KeyCode key)
        {
            foreach (InputEntry entry in entries)
            {
                if (entry.actionName != action) continue;
                entry.keyEntry = key;
                return;
            }
            Debug.LogWarning("The specified action name is unknown...");
        }
    }
}
