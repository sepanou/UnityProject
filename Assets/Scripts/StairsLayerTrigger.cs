using System;
using UnityEngine;

namespace Scenes.HubScene
{
    public class StairsLayerTrigger : MonoBehaviour
    {
        [SerializeField] private string destinationSortingLayer;
        [SerializeField] private string destinationLayerMask;
        private int _sortingLayerId;
        private int _layerMaskId;

        private void Start()
        {
            _sortingLayerId = SortingLayer.NameToID(destinationSortingLayer);
            _layerMaskId = LayerMask.NameToLayer(destinationLayerMask);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent(out SpriteRenderer otherRenderer))
            {
                otherRenderer.sortingLayerID = _sortingLayerId;
                other.gameObject.layer = _layerMaskId;
            }
            
            SpriteRenderer[] childRenderers = other.gameObject.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (var childRenderer in childRenderers)
            {
                childRenderer.sortingLayerID = _sortingLayerId;
                childRenderer.gameObject.layer = _layerMaskId;
            }
        }
    }
}
