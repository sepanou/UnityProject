using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Audio {
	public class ButtonCustom: Selectable {
		[SerializeField] private GameObject[] toActivate;
		[SerializeField] private RectTransform hoveringCanvas;
		[SerializeField] private Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();

		private readonly Vector3[] _worldCorners = new Vector3[4];
		private bool _isMouseOn, _isSelected;

		private new void Start() {
			if (!hoveringCanvas) {
				Destroy(this);
				return;
			}

			_isMouseOn = false;
			hoveringCanvas.GetWorldCorners(_worldCorners);
			SetTargetsActive(false);
		}

		private void SetTargetsActive(bool state) {
			foreach (GameObject obj in toActivate)
				obj.SetActive(state);
		}

		// Used by buttons - do not set private !
		public void OnSelect() {
			if (EventSystem.current)
				EventSystem.current.SetSelectedGameObject(gameObject);
		}
		
		public override void OnSelect(BaseEventData data) {
			_isSelected = true;
			SetTargetsActive(true);
			base.OnSelect(data);
		}

		// Used by buttons, do not set private or whatever !
		public void OnDeselect() {
			if (EventSystem.current)
				OnDeselect(new BaseEventData(EventSystem.current));
		}

		public override void OnDeselect(BaseEventData data) {
			_isSelected = false;
			SetTargetsActive(false);
			base.OnDeselect(data);
		}

		private void Update() {
			if (!MouseCursor.Instance || !MenuSettingsManager.Instance || MenuSettingsManager.Instance.isOpen) return;
			bool isOn = MouseCursor.Instance.IsMouseOn(this);
			if (!_isMouseOn && isOn) {
				OnSelect();
				_isMouseOn = true;
			} else if (_isMouseOn && !isOn)
				_isMouseOn = false;

			if (_isSelected && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)))
				onClick?.Invoke();
		}
	}
}
