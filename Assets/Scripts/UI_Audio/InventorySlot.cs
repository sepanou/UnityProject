using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio
{
    public class InventorySlot : MonoBehaviour
    {
        private static InventorySlot _previouslySelected = null;
        private static readonly Color Visible = new Color(255, 255, 255, 255);
        private static readonly Color Invisible = new Color(255, 255, 255, 0);
        
        [SerializeField] private Image slotImage;
        [SerializeField] private RectTransform hoveringCanvas;

        [Header("Transitions")]
        [SerializeField] private Image targetGraphic;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite normalSprite;
        
        [SerializeField] private Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();

        private bool _isMouseOver;
        private readonly Vector3[] _worldCorners = new Vector3[4];

        private void Start()
        {
            if (!hoveringCanvas || !targetGraphic || !slotImage)
            {
                Destroy(this);
                return;
            }

            _isMouseOver = false;
            hoveringCanvas.GetWorldCorners(_worldCorners);
            targetGraphic.sprite = normalSprite;
        }

        public void SetSlotItem(Sprite sprite)
        {
            if (!sprite)
            {
                ClearItem();
                return;
            }

            slotImage.sprite = sprite;
            slotImage.color = Visible;
        }

        public void ClearItem()
        {
            slotImage.sprite = null;
            slotImage.color = Invisible;
        }
        
        private void Update()
        {
            if (!MouseCursor.Instance || _previouslySelected == this) return;
            
            bool isOver = MouseCursor.Instance.IsMouseOver(_worldCorners);
            if (!_isMouseOver && isOver)
            {
                _isMouseOver = true;
                targetGraphic.sprite = hoverSprite;
            }
            else if (_isMouseOver && !isOver)
            {
                _isMouseOver = false;
                targetGraphic.sprite = normalSprite;
            }

            if (!_isMouseOver || !Input.GetMouseButtonDown(0)) return;
            
            if (_previouslySelected)
                _previouslySelected.targetGraphic.sprite = _previouslySelected.normalSprite;
                
            _previouslySelected = this;
            targetGraphic.sprite = selectedSprite;
                
            clickEvent?.Invoke();
        }
    }
}
