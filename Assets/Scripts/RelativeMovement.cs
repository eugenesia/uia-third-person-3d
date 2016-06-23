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

	// Store collision data between functions.
	private ControllerColliderHit _contact;

	private Animator _animator;


	// Use this for initialization
	void Start () {
		// Initialize vertical speed to min falling speed at start.
		// Keeps char pressed down and so it can run up and down on uneven terrain.
		_vertSpeed = minFall;

		_charController = GetComponent<CharacterController>();

		_animator = GetComponent<Animator>();
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

		// Use raycasting to detect if ground is touching character's feet,
		// as CharacterCollider is inaccurate for sloping ground.
		bool hitGround = false;

		RaycastHit hit;

		// Check if player is falling.
		if (_vertSpeed < 0 &&
			Physics.Raycast(transform.position, Vector3.down, out hit)) {

			// Distance to check against (extend slightly beyond bottom of capsule).
			//
			// The next several lines do raycasting. This code also goes below horizontal
			// movement but before the if statement for vertical movement. The actual
			// Physics.Raycast() call should be familiar from previous chapters, but the
			// specific parameters are different this time. Although the position to cast
			// a ray from is the same (the character’s position), the direction will be
			// down this time instead of forward. Then we check how far away the raycast
			// was when it hit something; if the distance of the hit is at the distance
			// of the character’s feet, then the character is standing on the ground, so
			// set hitGround to true.
			float check = (_charController.height + _charController.radius) / 1.9f;
			hitGround = hit.distance <= check;
		}

		_animator.SetFloat("Speed", movement.sqrMagnitude);

		// Handle jumps.

		// Check if char is on ground.
		if (hitGround) {
			// React to jump button only while on ground.
			if (Input.GetButtonDown("Jump")) {
				_vertSpeed = jumpSpeed;
			}
			else {
				_vertSpeed = -0.1f;
				// _vertSpeed = minFall;

				_animator.SetBool("Jumping", false);
			}
		}
		else {
			// If not on ground, apply gravity until terminal velocity reached.
			_vertSpeed += gravity * 5 * Time.deltaTime;
			if (_vertSpeed < terminalVelocity) {
				_vertSpeed = terminalVelocity;
			}

			// Prevent animator from playing jump animation right from the start.
			// The char falls down to the ground for a split second, don't need to play
			// animation.
			if (_contact != null) {
				_animator.SetBool("Jumping", true);
			}

			// Raycasting didn't detect ground, but capsule is touching ground.
			// E.g. when player walks off edge of platform.
			if (_charController.isGrounded) {

				// Respond slightly differently depending on whether character is
				// facing the contact point.
				//
				// Char not facing contact point. 
				if (Vector3.Dot(movement, _contact.normal) < 0) {
					// Replace movement to repel char away from contact point, and
					// so char won't keep moving in wrong direction.
					movement = _contact.normal * moveSpeed;
				}
				else {
					// Keep forward momentum away from edge.
					movement += _contact.normal * moveSpeed;
				}
			}
		}
		movement.y = _vertSpeed;

		// Multiply movement by deltaTime to make them frame rate-independent.
		movement *= Time.deltaTime;
		_charController.Move(movement);
	}

	// Store collision data in callback when collision detected.
	void OnControllerColliderHit(ControllerColliderHit hit) {
		_contact = hit;
	}
}
