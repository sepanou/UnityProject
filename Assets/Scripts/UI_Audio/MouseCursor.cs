﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Audio
{
    public class MouseCursor : MonoBehaviour
    {
        private Vector2 _lastPos, _lastCameraPos;
        private Animator _animator;
        private bool _isIdling;

        private void Start()
        {
            _lastPos = Input.mousePosition;
            Camera current = MenuSettingsManager.CurrentCamera;
            transform.position = current.ViewportToWorldPoint(
                ClampCoords(current.ScreenToViewportPoint(_lastPos))
            );
            _lastCameraPos = current.transform.position;
            Cursor.visible = false;
            TryGetComponent(out _animator);
            _isIdling = false;
        }

        private Vector2 ClampCoords(Vector2 viewportPoint)
        {
            float x = viewportPoint.x;
            float y = viewportPoint.y;
            return new Vector2(
                x < 0 ? 0 : x > 1 ? 1 : x,
                y < 0 ? 0 : y > 1 ? 1 : y
                );
        }

        private bool TryGetElementFromRayCast<T>(out T result)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) 
                {position = Input.mousePosition};
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            foreach (RaycastResult rayCast in results)
            {
                if (rayCast.gameObject.TryGetComponent(out result))
                    return true;
            }

            result = default;
            return false;
        }

        private IEnumerator CheckHovering(Selectable selectable)
        {
            if (!EventSystem.current || !selectable || !selectable.targetGraphic)
            {
                _isIdling = false;
                _animator.Play("Static");
                yield break;
            }
            
            _isIdling = true;
            _animator.Play("Idle");
            Vector3[] corners = new Vector3[4];
            selectable.targetGraphic.rectTransform.GetWorldCorners(corners);

            while (selectable && selectable.enabled)
            {
                Vector3 mousePos = transform.position;
                if (mousePos.x < corners[1].x || mousePos.x > corners[3].x) break;
                if (mousePos.y < corners[0].y || mousePos.y > corners[2].y) break;
                yield return new WaitForSeconds(0.1f);
            }

            _isIdling = false;
            _animator.Play("Static");
        }
        
        private void SetAnimation()
        {
            if (_isIdling || !_animator || !EventSystem.current) return;
            if (!TryGetElementFromRayCast(out Selectable selectable)) return;

            StopAllCoroutines();
            StartCoroutine(CheckHovering(selectable));
        }

        private void Update()
        {
            if (MenuSettingsManager.Instance && MenuSettingsManager.Instance.isOpen)
                SetAnimation();
            Vector2 newPos = Input.mousePosition;
            Vector2 newCameraPos = MenuSettingsManager.CurrentCamera.transform.position;
            if (newPos == _lastPos && _lastCameraPos == newCameraPos) return;
            _lastPos = newPos;
            _lastCameraPos = newCameraPos;
            newPos = ClampCoords(MenuSettingsManager.CurrentCamera.ScreenToViewportPoint(newPos));
            transform.position = (Vector2) MenuSettingsManager.CurrentCamera.ViewportToWorldPoint(newPos);
        }
    }
}
