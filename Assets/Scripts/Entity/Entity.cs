using UnityEngine;

namespace Entity
{
    public abstract class Entity : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        
        protected void InstantiateEntity()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        
    }
}
