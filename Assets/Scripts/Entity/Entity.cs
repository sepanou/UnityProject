using UnityEngine;

namespace Entity
{
    public abstract class Entity : MonoBehaviour
    {
        private SpriteRenderer Renderer;

        protected void InstantiateEntity()
        {
            Renderer = GetComponent<SpriteRenderer>();
        }

        public Vector2 GetPosition2D()
        {
            return transform.position;
        }

        public void SetPosition2D(Vector2 newPos)
        {
            transform.position = newPos;
        }

        public SpriteRenderer GetSpriteRenderer()
        {
            return Renderer;
        }
        
    }
}
