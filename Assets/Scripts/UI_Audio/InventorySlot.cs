using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio {
	public class InventorySlot: MonoBehaviour {
		private static InventorySlot _previouslySelected = null;
		private static readonly Color Visible = new Color(255, 255, 255, 255);
		private static readonly Color Invisible = new Color(255, 255, 255, 0);
		
		[SerializeField] private Image slotImage;
		[SerializeField] private RectTransform hoveringCanvas;
		[SerializeField] private RectTransform infoDisplay;

		[Header("Transitions")]
		[SerializeField] private Image targetGraphic;
		[SerializeField] private Sprite hoverSprite;
		[SerializeField] private Sprite selectedSprite;
		[SerializeField] private Sprite normalSprite;
		
		[SerializeField] private Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
		[SerializeField] private Toggle.ToggleEvent containsItemEvent = new Toggle.ToggleEvent();

		[NonSerialized] public bool IsOccupied;
		private bool _isMouseOver;
		private IInventoryItem _item;

		private void Start() {
			if (!hoveringCanvas || !targetGraphic || !slotImage) {
				Destroy(this);
				return;
			}
			_isMouseOver = false;
			targetGraphic.sprite = normalSprite;
			infoDisplay.gameObject.SetActive(false);
		}

		public void SetSlotItem(IInventoryItem item) {
			slotImage.sprite = item.GetSpriteRenderer().sprite;
			slotImage.color = Visible;
			_item = item;
			if (!IsOccupied)
				containsItemEvent?.Invoke(true);
			IsOccupied = true;
		}

		public IInventoryItem GetSlotItem() => _item;
		
		public void ClearItem() {
			slotImage.sprite = null;
			slotImage.color = Invisible;
			_item = null;
			if (IsOccupied)
				containsItemEvent?.Invoke(false);
			IsOccupied = false;
		}

		private void SetInfoDisplayActive(bool state) {
			if (state) {
				RectTransform info = _item.GetInformationPopup();
				info.gameObject.transform.SetParent(infoDisplay.transform, false);
			}
			infoDisplay.gameObject.SetActive(state);
		}

		private void Update() {
			if (!MouseCursor.Instance || _previouslySelected == this) return;
			bool isOver = MouseCursor.Instance.IsMouseOver(hoveringCanvas);
			if (!_isMouseOver && isOver) {
				_isMouseOver = true;
				targetGraphic.sprite = hoverSprite;
			} else if (_isMouseOver && !isOver) {
				SetInfoDisplayActive(false);
				_isMouseOver = false;
				targetGraphic.sprite = normalSprite;
			} else if (_isMouseOver && _item != null)
				SetInfoDisplayActive(true);
			if (!_isMouseOver || !Input.GetMouseButtonDown(0)) return;
			if (_previouslySelected)
				_previouslySelected.targetGraphic.sprite = _previouslySelected.normalSprite;
			_previouslySelected = this;
			targetGraphic.sprite = selectedSprite;
			clickEvent?.Invoke();
		}
	}
}
