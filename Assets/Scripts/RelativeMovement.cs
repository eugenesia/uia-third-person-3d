using UnityEngine;
using System.Collections;

// Allow player character to move relative to screen, i.e.
// "left" means move to left of screen/camera.
public class RelativeMovement : MonoBehaviour {

	// Reference to object to move relative to (camera in this case).
	[SerializeField] private Transform target;

	// Apply linear interpolation speed to character's rotation so it doesn't
	// snap abruptly.
	public float rotSpeed = 15.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// Start with vector (0,0,0) and add movement components progressively.
		//  It’s important to create a zeroed vector and fill in the values later
		// rather than simply create a vector later with the movement values calculated,
		// because the vertical and horizontal movement values will be calculated in
		// different steps and yet they all need to be part of the same vector.
		Vector3 movement = Vector3.zero;

		float horInput = Input.GetAxis("Horizontal");
		float vertInput = Input.GetAxis("Vertical");

		// Direction key is being pressed.
		// Input.GetAxis() returns 0 if no button is pressed, and it varies between 1 and –1
		// when those keys are being pressed.
		if (horInput != 0 || vertInput != 0) {
			movement.x = horInput;
			movement.z = vertInput;

			// Keep initial rotation to restore after finishing with target object.
			Quaternion tmp = target.rotation;
			target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);

			// Transform movement direction from local to global coords.
			movement = target.TransformDirection(movement);
			target.rotation = tmp;

			// Final direction of character movement.
			Quaternion direction = Quaternion.LookRotation(movement);

			// Apply a bit of movement direction to character, but use linear interpolation
			// so it doesn't rotate abruptly.
			transform.rotation = Quaternion.Lerp(transform.rotation,
				direction, rotSpeed * Time.deltaTime);
		}
	}
}
