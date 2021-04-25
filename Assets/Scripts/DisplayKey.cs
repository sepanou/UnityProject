using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DisplayKey: MonoBehaviour {
	private Image _image;
	private bool _displaying;
	private bool _running;
	[SerializeField] private Sprite spr1;
	[SerializeField] private Sprite spr2;

	private void Start() {
		_image = GetComponent<Image>();
		_image.color = new Color(255,255,255,0);
	}

	public void StartDisplay() {
		_image.color = new Color(255, 255, 255, 100);
		_displaying = true;
	}

	public void StopDisplay() {
		_image.color = new Color(255,255,255,0);
		_displaying = false;
	}

	private IEnumerator DisplayCoroutine() {
		while (_displaying) {
			_image.sprite = spr1;
			yield return new WaitForSeconds(0.5f);
			_image.sprite = spr2;
			yield return new WaitForSeconds(0.5f);
			yield return null;
		}
	}
	private void Update() {
		if (_displaying && !_running) {
			StartCoroutine(DisplayCoroutine());
			_running = true;
		}

		if (!_displaying && _running) {
			_running = false;
			StopAllCoroutines();
		}
	}
}
