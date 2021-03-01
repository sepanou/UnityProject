using UnityEngine;

public class Character : MonoBehaviour
{

    private float speed = 0.125f;
    private Quaternion transformRotation;
    private Vector3 lastPos;
    private GameObject camera;
    public bool facingRight;

    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
        camera = GameObject.Find("Main Camera");
        facingRight = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        //WalkingAnimation();
        WhereToFace();
        transform.position += move * speed;
    }

    public void switchSide(bool switching)
    {
        facingRight = switching;
    }

    void WhereToFace()
    {
        if (GetComponentInChildren<SwordAttack>().isAttacking)
        {
            return;
        }

        transformRotation = transform.rotation;
        if (facingRight)
        {
            transformRotation.y = 0f;
        }
        else
        {
            transformRotation.y = 180f;
        }

        lastPos = transform.position;
        camera.transform.parent = null;
        transform.rotation = transformRotation;
        camera.transform.parent = this.transform;
    }

    /*
        void WalkingAnimation()
        {
            if (transform.position == lastPos|| GetComponentInChildren<SwordAttack>().isAttacking)
            {
                return;
            }
            transformRotation = transform.rotation;
            Vector3 displacement = (Vector3) transform.position - lastPos;
            if (transform.position.x > lastPos.x)
            {
                transformRotation.y = 0f;
            }
            else
            {
                transformRotation.y = 180f;
            }
            lastPos = transform.position;
            camera.transform.parent = null;
            transform.rotation = transformRotation;
            camera.transform.parent = this.transform;if (transform.position.x > lastPos.x)
            {
                transformRotation.y = 0f;
            }
            else
            {
                transformRotation.y = 180f;
            }
            lastPos = transform.position;
            camera.transform.parent = null;
            transform.rotation = transformRotation;
            camera.transform.parent = this.transform;
        }*/
}