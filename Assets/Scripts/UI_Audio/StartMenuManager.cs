using System;
using UnityEngine;

namespace UI_Audio
{
    public class StartMenuManager : MonoBehaviour
    {
        // This class should deal with everything related to the start menu and notably the
        // start of a game (goal = player prefab instantiated if everything goes right).

        [Header("Fields")]
        [SerializeField] private RectTransform defaultFields;
        [SerializeField] private RectTransform gameModeFields;
        [SerializeField] private RectTransform multiPlayerFields;
        [SerializeField] private RectTransform pseudoFields;
        
        public static StartMenuManager Instance;
        
        private void OnEnable()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            gameModeFields.gameObject.SetActive(false);
            multiPlayerFields.gameObject.SetActive(false);
            pseudoFields.gameObject.SetActive(false);
        }
    }
}
