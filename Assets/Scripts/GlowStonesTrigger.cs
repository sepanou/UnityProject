using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowStonesTrigger: MonoBehaviour {
	[SerializeField] private GameObject[] glowingSymbols;
	[SerializeField] private float delay;
	private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();

	private void Start() {
		foreach (GameObject symbol in glowingSymbols)
			if (symbol.TryGetComponent(out SpriteRenderer spriteRenderer))
				_renderers.Add(spriteRenderer);
	}

	private void SetColors(float alpha) {
		foreach (SpriteRenderer spriteRenderer in _renderers) {
			Color color = spriteRenderer.color;
			color = new Color(color.r, color.g, color.b, alpha);
			spriteRenderer.color = color;
		}
	}
	
	IEnumerator Appear() {
		if (_renderers.Count <= 0)
			yield break;
		
		for (float a = _renderers[0].color.a; a < 1f; a += 0.1f) {
			SetColors(a);
			yield return new WaitForSeconds(delay);
		}
	}

	IEnumerator Disappear() {
		if (_renderers.Count <= 0)
			yield break;
		
		for (float a = _renderers[0].color.a; a > 0f; a -= 0.1f) {
			SetColors(a);
			yield return new WaitForSeconds(delay);
		}
	}
	
	private void OnTriggerEnter2D(Collider2D other) {
		StopAllCoroutines();
		StartCoroutine(Appear());
	}

	private void OnTriggerExit2D(Collider2D other) {
		StopAllCoroutines();
		StartCoroutine(Disappear());
	}
}