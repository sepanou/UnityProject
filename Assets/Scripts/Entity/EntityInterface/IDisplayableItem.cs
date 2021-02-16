using UnityEngine;

namespace Entity.EntityInterface
{
    public interface IDisplayableItem
    {
        // Maybe useless... Read-only variable
        string Name { get; }
        // Sprite displayed on the slot may be different from the original sprite - Read-only variable
        Sprite DisplayedSprite { get; }
        
        // Behavior when the item is selected
        void OnSelect<T>(T source);
        // Behavior when the item is used ('Fire1')
        void OnUse<T>(T source);
        // Behavior when the item is no longer selected
        void OnDeselect<T>(T source);
        // Behavior when the item is dropped on the ground
        void OnDrop<T>(T source);
    }
}