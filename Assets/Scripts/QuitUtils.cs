using UnityEngine;
using UnityEditor;

public class QuitUtils: MonoBehaviour {
	public void Quit() {
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	private void Update() {
		if (Input.GetKey("escape"))
			Quit();
	}
}
