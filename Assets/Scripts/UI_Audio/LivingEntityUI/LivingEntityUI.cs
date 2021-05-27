using System;
using Entity.DynamicEntity.LivingEntity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Audio.LivingEntityUI {
    public class LivingEntityUI : MonoBehaviour {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TMP_Text nameTagField;

        private LivingEntity _target;
        private Vector2 _lastViewportPos, _dimensions;
        private Camera _worldCamera;

        public void Initialize(LivingEntity entity) {
            _target = entity;
            _worldCamera = LocalGameManager.Instance.worldCamera;
            _lastViewportPos = new Vector2Int(-1, -1);
            _dimensions = rectTransform.anchorMax;
        }
        
        public void SetHealthBarValue(float amount) => healthBar.value = amount;

        public void SetNameTagField(string nameTag) => nameTagField.text = nameTag;

        public void Destroy() => Destroy(gameObject);
        
        private void FixedUpdate() {
            if (!LocalGameManager.Instance || !_target) return;
            
            Vector2 viewportPos = _worldCamera.WorldToViewportPoint(_target.Position + new Vector2(-1f, 1.25f));
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                canvasGroup.alpha = 0;
                return;
            }

            canvasGroup.alpha = 1;
            viewportPos.Set((float) Math.Round(viewportPos.x, 5),
                (float) Math.Round(viewportPos.y, 5));
            if (_lastViewportPos == viewportPos) return;
            _lastViewportPos = viewportPos;
            rectTransform.anchorMin = viewportPos;
            rectTransform.anchorMax = _dimensions + viewportPos;
        }
    }
}
