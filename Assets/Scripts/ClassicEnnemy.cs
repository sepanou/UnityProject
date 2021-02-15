using UnityEngine;

public class ClassicEnemy: MonoBehaviour {
	public Quaternion transformRotation;
	public Vector3 lastPos;
	public Animator animator;
	public GameObject player;

	private void Start() {
		lastPos = transform.position;
	}
	
	private void Update() {
		animator.SetBool("IsWalking", HasMoved());
		transform.position = Vector3.MoveTowards(transform.position, player.transform.position, .03f);
	}

	private bool HasMoved() {
		transformRotation = transform.rotation;
		Vector3 displacement = transform.position - lastPos;
		transformRotation.y = transform.position.x < lastPos.x ? 0f : 180f;
		lastPos = transform.position;
		transform.rotation = transformRotation;
		return displacement.magnitude > 0.00001;
	}
}
