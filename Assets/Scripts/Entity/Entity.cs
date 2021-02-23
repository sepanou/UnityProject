using UnityEngine;

namespace Entity
{
    public abstract class Entity : MonoBehaviour
    {
        protected SpriteRenderer Renderer;
        
        protected void InstantiateEntity()
        {
            Renderer = GetComponent<SpriteRenderer>();
        }
        
    }
}
