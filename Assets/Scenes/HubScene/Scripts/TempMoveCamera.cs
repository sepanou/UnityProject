using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMoveCamera : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private float velocity;
    
    void Start()
    {
        _transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        _transform.position += new Vector3(x, y, 0) * (velocity * Time.deltaTime);
    }
}
