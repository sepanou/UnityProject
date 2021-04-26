using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Audio {
	public class MouseCursor: MonoBehaviour {
		[NonSerialized] public static MouseCursor Instance;

		[SerializeField] private RenderTexture renderTexture;
		private Vector2 _lastPos; // Screen coords (px)
		private Vector2 _lastViewportPos; // Viewport coords
		private Vector2Int _windowResolution;
		private Animator _animator;
		private bool _isIdling;
		private Camera _mouseCamera;

		private void Awake() {
			if (!Instance)
				Instance = this;
			else throw new Exception("created two instances of MouseCursor");
		}

		public void Initialize() {
			_windowResolution = new Vector2Int(Screen.width, Screen.height);
			_mouseCamera = LocalGameManager.Instance.mouseAndParticlesCamera;

			ResetRenderTexture();
			
			_lastPos = Input.mousePosition;
			_lastViewportPos = ClampCoords(_mouseCamera.ScreenToViewportPoint(_lastPos));
			transform.position = _mouseCamera.ViewportToWorldPoint(_lastViewportPos);
			Cursor.visible = false;
			TryGetComponent(out _animator);
			_isIdling = false;
			
			gameObject.SetActive(true);
		}

		public bool IsMouseOver(RectTransform rect)
			=> RectTransformUtility.RectangleContainsScreenPoint(rect, _lastPos);

		public bool IsMouseOn(Selectable selectable) 
			=> TryGetElementFromRayCast(out Selectable result) && result == selectable;
		
		public Vector3 GetLocalViewWorldCoords() => transform.position;

		public Quaternion OrientateObjectTowardsMouse(Vector3 referenceDirection, out Vector2 orientation) {
			orientation = transform.position - _mouseCamera.transform.position;
			orientation.Normalize();
			return Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(referenceDirection, orientation)));
		}

		private void ResetRenderTexture() {
			renderTexture.Release();
			renderTexture.width = _windowResolution.x;
			renderTexture.height = _windowResolution.y;
			renderTexture.Create();
		}
		
		private Vector2 ClampCoords(Vector2 viewportPoint) {
			// Viewport coords => between 0 and 1 if inside the game window
			float x = viewportPoint.x;
			float y = viewportPoint.y;
			return new Vector2(
				x < 0 ? 0 : x > 1 ? 1 : x,
				y < 0 ? 0 : y > 1 ? 1 : y
			);
		}

		private bool TryGetElementFromRayCast<T>(out T result) {
			result = default;
			EventSystem system = EventSystem.current;
			if (!system) return false;
			PointerEventData pointerData = new PointerEventData(system) { position = Input.mousePosition };
			List<RaycastResult> results = new List<RaycastResult>();
			system.RaycastAll(pointerData, results);
			// The first object is the "closest" to the pointer, the others are "behind".
			return results.Count != 0 && results[0].gameObject.TryGetComponent(out result);
		}

		private IEnumerator CheckHovering(Selectable selectable) {
			if (!EventSystem.current || !selectable || !selectable.targetGraphic) {
				_isIdling = false;
				_animator.Play("Static");
				yield break;
			}
			
			_isIdling = true;
			_animator.Play("Idle");
			
			while (selectable && selectable.enabled) {
				if (!IsMouseOn(selectable)) break;
				yield return new WaitForSeconds(0.1f);
			}

			_isIdling = false;
			_animator.Play("Static");
		}
		
		private void SetAnimation() {
			if (_isIdling || !_animator || !EventSystem.current) return;
			if (!TryGetElementFromRayCast(out Selectable selectable)) return;

			StopAllCoroutines();
			StartCoroutine(CheckHovering(selectable));
		}

		private void Update() {
			int width = Screen.width;
			int height = Screen.height;
			
			if (_windowResolution.x != width || _windowResolution.y != height) {
				// Window size has changed!
				_windowResolution.Set(width, height);
				ResetRenderTexture();
			}
			
			SetAnimation();
			Vector2 newPos = Input.mousePosition;
			if (newPos == _lastPos) return;
			_lastPos = newPos;
			_lastViewportPos = ClampCoords(_mouseCamera.ScreenToViewportPoint(_lastPos));
			transform.position = (Vector2) _mouseCamera.ViewportToWorldPoint(_lastViewportPos);
		}
	}
}
