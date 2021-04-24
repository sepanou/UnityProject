using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio
{
    public class ButtonCustom : Selectable
    {
        [SerializeField] private GameObject[] toActivate;
        [SerializeField] private RectTransform hoveringCanvas;
        [SerializeField] private Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();
        
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private bool _isMouseOver;
        
        private new void Start()
        {
            if (!hoveringCanvas)
            {
                Destroy(this);
                return;
            }

            _isMouseOver = false;
            hoveringCanvas.GetWorldCorners(_worldCorners);
            SetTargetsActive(false);
        }

        private void SetTargetsActive(bool state)
        {
            foreach (GameObject obj in toActivate)
                obj.SetActive(state);
        }

        private void Update()
        {
            if (!MouseCursor.Instance || !MenuSettingsManager.Instance || MenuSettingsManager.Instance.isOpen) return;
            
            bool isOver = MouseCursor.Instance.IsMouseOver(hoveringCanvas);
            if (!_isMouseOver && isOver)
            {
                _isMouseOver = true;
                SetTargetsActive(true);
            }
            else if (_isMouseOver && !isOver)
            {
                _isMouseOver = false;
                SetTargetsActive(false);
            }
            
            if (_isMouseOver && Input.GetMouseButtonDown(0))
                onClick?.Invoke();
        }
    }
}
