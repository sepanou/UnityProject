using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio {
	public class ButtonCustom: Selectable {
		[SerializeField] private GameObject[] toActivate;
		[SerializeField] private RectTransform hoveringCanvas;
		[SerializeField] private Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();
		
		private readonly Vector3[] _worldCorners = new Vector3[4];
		private bool _isMouseOn;
		
		private new void Start() {
			if (hoveringCanvas is null) {
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

		private void Update() {
			if (!MouseCursor.Instance || !MenuSettingsManager.Instance || MenuSettingsManager.Instance.isOpen) return;
			bool isOn = MouseCursor.Instance.IsMouseOn(this);
			if (!_isMouseOn && isOn) {
				_isMouseOn = true;
				SetTargetsActive(true);
			} else if (_isMouseOn && !isOn) {
				_isMouseOn = false;
				SetTargetsActive(false);
			}
			if (_isMouseOn && Input.GetMouseButtonDown(0))
				onClick?.Invoke();
		}
	}
}
