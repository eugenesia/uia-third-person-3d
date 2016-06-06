using UnityEngine;
using System.Collections;

// Orbit the camera around the player character.
public class OrbitCamera : MonoBehaviour {

	// Serialized ref to the object to orbit around.
	[SerializeField] private Transform target;

	public float rotSpeed = 1.5f;

	private float _rotY;
	private Vector3 _offset;

	// Use this for initialization
	void Start () {
		_rotY = transform.eulerAngles.y;
		// Store starting position offset between camera and target.
		_offset = target.position - transform.position;	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Called every frame but after Update(). Ensure camera updates after target has
	// moved.
	void LateUpdate() {
		float horInput = Input.GetAxis("Horizontal");

		// Either rotate the camera slowly using arrow keys...
		if (horInput != 0) {
			_rotY += horInput * rotSpeed;
		}
		// Or rotate quickly with mouse.
		else {
			_rotY += Input.GetAxis("Mouse X") * rotSpeed * 3;
		}

		Quaternion rotation = Quaternion.Euler(0, _rotY, 0);

		// Maintain the starting offset, shifted according to the camera's rotation.
		transform.position = target.position - (rotation * _offset);

		// No matter where the camera is relative to target, always face target.
		transform.LookAt(target);
	}
}
