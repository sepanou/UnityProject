using UnityEngine;

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
        GameObject colliderObject = other.gameObject;
        
        if (colliderObject.layer == _layerMaskId)
            return;
        
        Entity.Entity.SetRenderingLayersInChildren(_sortingLayerId, destinationSortingLayer, _layerMaskId, colliderObject);
    }
}