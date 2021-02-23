using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.HubScene
{
    public class GlowStonesTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject[] glowingSymbols;
        [SerializeField] private float delay;
        private List<SpriteRenderer> _renderers;
        private IEnumerator _appearCoroutine, _disappearCoroutine;
        private bool _isAppearAlive, _isDisappearAlive;

        void Start()
        {
            _renderers = new List<SpriteRenderer>();

            foreach (GameObject symbol in glowingSymbols)
            {
                if (symbol.TryGetComponent(out SpriteRenderer spriteRenderer))
                    _renderers.Add(spriteRenderer);
            }
        
            _isAppearAlive = false;
            _isDisappearAlive = false;
        }

        private void SetColors(float alpha)
        {
            foreach (SpriteRenderer spriteRenderer in _renderers)
            {
                Color color = spriteRenderer.color;
                color = new Color(
                    color.r,
                    color.g,
                    color.b,
                    alpha);
                spriteRenderer.color = color;
            }
        }
    
        IEnumerator Appear()
        {
            if (_renderers.Count <= 0)
                yield break;
        
            for (float a = _renderers[0].color.a; a < 1f; a += 0.1f)
            {
                SetColors(a);
                yield return new WaitForSeconds(delay);
                if (!_isAppearAlive)
                    yield break;
            }
        }

        IEnumerator Disappear()
        {
            if (_renderers.Count <= 0)
                yield break;
        
            for (float a = _renderers[0].color.a; a > 0f; a -= 0.1f)
            {
                SetColors(a);
                yield return new WaitForSeconds(delay);
                if (!_isDisappearAlive)
                    yield break;
            }
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            _isAppearAlive = true;
            _isDisappearAlive = false;
            _appearCoroutine = Appear();
            StartCoroutine(_appearCoroutine);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _isAppearAlive = false;
            _isDisappearAlive = true;
            _disappearCoroutine = Disappear();
            StartCoroutine(_disappearCoroutine);
        }
    }
}
