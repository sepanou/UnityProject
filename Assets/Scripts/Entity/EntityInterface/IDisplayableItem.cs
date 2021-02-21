using UnityEngine;

namespace Entity.EntityInterface
{
    public interface IDisplayableItem
    {
        // Maybe useless... Read-only variable
        string Name { get; }
        // Sprite displayed on the slot may be different from the original sprite - Read-only variable
        Sprite DisplayedSprite { get; }
        
        /// <summary>
        /// Behavior when the item is selected (ex: the entity <paramref name="source"/> is holding it)
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        void OnSelect<T>(T source);
        
        /// <summary>
        /// Behavior when the item is used by <paramref name="source"/> (ex: 'Fire1' button pressed)
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        void OnUse<T>(T source);

        /// <summary>
        /// Behavior when the item is deselected (ex: <paramref name="source"/> is no longer holding it)
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        void OnDeselect<T>(T source);

        /// <summary>
        /// Behavior when the item is dropped by <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        void OnDrop<T>(T source);
    }
}