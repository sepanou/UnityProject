using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DataBanks {
    using PlayerData = Dictionary<string, string>;

    public static class FileStorage {
        private static readonly string FILE_PATH = Application.dataPath + "/players.json";

        public static int GetPlayerOrchid(string username) {
            Debug.Log(FILE_PATH);
            CreateFileIfNotExist();
            Dictionary<string, PlayerData> accounts = LoadUserAccounts();

            return accounts.ContainsKey(username) && accounts[username].ContainsKey("orchid")
                ? int.Parse(accounts[username]["orchid"])
                : 0;
        }


        public static void SavePlayerOrchid(string username, int value) {
            CreateFileIfNotExist();
            Dictionary<string, PlayerData> accounts = LoadUserAccounts();
            if (!accounts.ContainsKey(username))
                accounts[username] = new PlayerData();
            accounts[username]["orchid"] = value.ToString();
            SaveUserAccounts(accounts);
        }

        private static Dictionary<string, PlayerData> LoadUserAccounts() =>
            JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(File.ReadAllText(FILE_PATH));

        private static void SaveUserAccounts(Dictionary<string, PlayerData> accounts) =>
            File.WriteAllText(FILE_PATH, JsonConvert.SerializeObject(accounts, Formatting.Indented));
        
        
        private static void CreateFileIfNotExist() {
            if (File.Exists(FILE_PATH)) return;
            // Have to put it that way so Unity accepts it, I know it sucks....
            // ReSharper disable once ConvertToUsingDeclaration
            using (StreamWriter streamWriter = File.CreateText(FILE_PATH)) { // Have to put it that way so Unity accepts it, I know it sucks....
                streamWriter.Write(JsonConvert.SerializeObject(new Dictionary<string, PlayerData>()));
            }
            
        }
    }
}