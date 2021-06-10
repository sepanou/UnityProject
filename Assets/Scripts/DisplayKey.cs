using System.Collections;
using DataBanks;
using UnityEngine;
using UnityEngine.UI;

public class DisplayKey: MonoBehaviour {
	private bool _displaying;
	private bool _running;
	private Sprite _spr1;
	private Sprite _spr2;
	[SerializeField] private InputManager inputManager;
	[SerializeField] private InputTexturesDB inputTexturesDB;
	[SerializeField] private Image image;
	private bool _keyNotFound;

	private void Start() {
		image = GetComponent<Image>();
		StopDisplay();
		InputTexture inputTexture = inputTexturesDB.Textures[inputManager.GetKeyCode("Interact").ToString()];
		_spr1 = inputTexture.sprite1;
		_spr2 = inputTexture.sprite2;

	}

	public void StartDisplay() {
		image.color = new Color(255, 255, 255, 100);
		_displaying = true;
	}

	public void StopDisplay() {
		image.color = new Color(255,255,255,0);
		_displaying = false;
	}

	private IEnumerator DisplayCoroutine() {
		while (_displaying) {
			if (_keyNotFound) image.color = new Color(255, 255, 255, 0);
			else image.color = new Color(255, 255, 255, 100);
			image.sprite = _spr1;
			yield return new WaitForSeconds(0.5f);
			image.sprite = _spr2;
			yield return new WaitForSeconds(0.5f);
			yield return null;
		}
	}
	private void Update() {
		try {
			InputTexture inputTexture = inputTexturesDB.Textures[inputManager.GetKeyCode("Interact").ToString()];
			_spr1 = inputTexture.sprite1;
			_spr2 = inputTexture.sprite2;
			_keyNotFound = false;
		}
		catch {
			_keyNotFound = true;
		}

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
