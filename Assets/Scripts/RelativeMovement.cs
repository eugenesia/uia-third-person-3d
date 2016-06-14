using UnityEngine;
using System.Collections;

// Ensure that player car has CharacterController component.
[RequireComponent(typeof(CharacterController))]

// Allow player character to move relative to screen, i.e.
// "left" means move to left of screen/camera.
public class RelativeMovement : MonoBehaviour {

	// Reference to object to move relative to (camera in this case).
	[SerializeField] private Transform target;

	// Apply linear interpolation speed to character's rotation so it doesn't
	// snap abruptly.
	public float rotSpeed = 15.0f;

	public float moveSpeed = 6.0f;

	private CharacterController _charController;

	// Jumping-related properties.
	public float jumpSpeed = 15.0f;
	public float gravity = -9.8f;
	public float terminalVelocity = -10.0f;
	public float minFall = -1.5f;

	private float _vertSpeed;


	// Use this for initialization
	void Start () {
		// Initialize vertical speed to min falling speed at start.
		// Keeps char pressed down and so it can run up and down on uneven terrain.
		_vertSpeed = minFall;

		_charController = GetComponent<CharacterController>();
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

			// Limit diagonal movement to the same speed as movement along an axis.
			movement = Vector3.ClampMagnitude(movement, moveSpeed);

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


		// Handle jumps.

		// Check if char is on ground.
		if (_charController.isGrounded) {
			// React to jump button only while on ground.
			if (Input.GetButtonDown("Jump")) {
				_vertSpeed = jumpSpeed;
			}
			else {
				_vertSpeed = minFall;
			}
		}
		else {
			// If not on ground, apply gravity until terminal velocity reached.
			_vertSpeed += gravity * 5 * Time.deltaTime;
			if (_vertSpeed < terminalVelocity) {
				_vertSpeed = terminalVelocity;
			}
		}
		movement.y = _vertSpeed;

		// Multiply movement by deltaTime to make them frame rate-independent.
		movement *= Time.deltaTime;
		_charController.Move(movement);
	}
}
