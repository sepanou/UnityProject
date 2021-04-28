using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Audio {
    public class ToSelectButton : MonoBehaviour {
        [SerializeField] private GameObject toSelect;
        private Button _button;

        private void Start() => TryGetComponent(out _button);

        // Used via the inspector
        public void SetToSelect(GameObject target) => toSelect = target;

        private void Update() {
            if (!Input.GetKeyDown(KeyCode.Return)) return;
            _button.onClick?.Invoke();
            if (toSelect && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(toSelect);
        }
    }
}