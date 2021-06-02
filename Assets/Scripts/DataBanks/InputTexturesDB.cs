using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataBanks{
    [CreateAssetMenu(fileName = "InputTexturesDB", menuName = "DataBanks/InputTexturesDB", order = 1)]
    public class InputTexturesDB: ScriptableObject{
        public InputTexture[] entries;
        public readonly Dictionary<string, InputTexture> Textures = new Dictionary<string, InputTexture>();

        public void SetEntries() {
            foreach (InputTexture inputTexture in entries) {
                Textures.Add(inputTexture.keyName, inputTexture);
            }
        }
    }

    [Serializable]
    public class InputTexture{
        public string keyName;
        public KeyCode key;
        public Sprite sprite1;
        public Sprite sprite2;
    }
}
