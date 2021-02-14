using UnityEngine;

public class QuitUtils: MonoBehaviour {
	public void Quit() {
		Application.Quit();
	}

	private void Update() {
		if (Input.GetKey("escape"))
			Quit();
	}
}
