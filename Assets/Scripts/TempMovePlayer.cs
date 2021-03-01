using UnityEngine;

public class TempMovePlayer : MonoBehaviour
{
    private Rigidbody2D _body;
    [SerializeField] private float velocity;
    
    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(x, y);
        direction.Normalize();
        _body.velocity = velocity * direction;
    }
}