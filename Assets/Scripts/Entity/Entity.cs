using Mirror;
using UnityEngine;

namespace Entity
{
    public abstract class Entity : NetworkBehaviour
    {
        protected SpriteRenderer Renderer;
        
        protected void InstantiateEntity()
        {
            Renderer = GetComponent<SpriteRenderer>();
        }

        public Vector2 GetPosition2D() => transform.position;
        
        public SpriteRenderer GetSpriteRenderer() => Renderer;

        [ServerCallback]
        public void SetPosition2D(Vector2 newPos) => transform.position = newPos;
    }
}
