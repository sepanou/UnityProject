using UnityEngine;

namespace DataBanks {
    [CreateAssetMenu(fileName = "MobsPrefabDB", menuName = "DataBanks/MobsPrefabDB", order =  1)]
    public class MobsPrefabsDB : ScriptableObject{ 
        public GameObject[] mobsPrefabs;
    }
}
