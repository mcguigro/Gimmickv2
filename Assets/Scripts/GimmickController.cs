using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickController : MonoBehaviour {

	// Physics-related constants
	public const float MAXSPEED = 10f;				// The max horizontal speed (units/sec) at which Gimmick can travel
	public const float NORMALACCEL = 20f;			// The acceleration (units/sec^2) experienced by Gimmick when speeding up
	public const float SKIDDECEL = 40f;				// The acceleration (units/sec^2) experienced by Gimmick when slowing down
	public const float COASTDECEL = 20f;			// The deceleration (units/sec^2) experienced by Gimmick while grounded and while left/right input is neutral
	public const float INITIALJUMPSPEED = 15f;		// Gimmick's initial upward velocity (units/sec) at the start of a jump
	public const float MAXJUMPDURATION = 0.3f;		// The maximum amount of time (sec) for which the player can hold the jump button after a jump to keep gravity at GRAVITYWHILEJUMPING
	public const float GRAVITYWHILEJUMPING = 3f;	// The gravity scale for Gimmick while the player holds the jump button after a jump, for up to MAXJUMPDURATION time
	public const float GRAVITYWHILEFALLING = 8f;	// The normal gravity scale for Gimmick, for when the conditions for GRAVITYWHILEJUMPING do not apply
	public const float MAXFALLSPEED = 25f;			// The max speed (units/sec) at which Gimmick can fall

	// Other constants
	public const float FLOATTOLERANCE = 0.00001f;	// Tolerance for comparing two floating point numbers, to accomodate for blips in Unity's physics engine

	// Enum for xInput (left/right input) values
	public const int LEFT = -1;
	public const int NEUTRAL = 0;
	public const int RIGHT = 1;

	//Character status-related variables
	public int xInput;					// Indicates whether the player is pressing left, right, or neither; uses above LEFT/NEUTRAL/RIGHT enum
	public bool isGrounded;				// Indicates whether Gimmick is standing on something
	public float currentJumpDuration;	// The amount of time (sec) for which the player has held the jump button following a jump to decrease gravity.  Is 0 when normal gravity applies.

	// Variables for checking if grounded
	public Transform groundCheck;		// Center of ground-collision box, **is initialized in Unity main window**
	public Vector2 groundCheckDim;		// Dimensions of ground-collision box, **is initialized in Unity main window**
	public LayerMask whatIsGround;		// LayerMask for ground, **is initialized in Unity main window**

	// Various Unity components
	private Rigidbody2D myRigidbody;
	private Animator myAnim;


	// Use this for initialization
	void Start () {
		// Initialize variables to default values, get components
		xInput = NEUTRAL;
		currentJumpDuration = 0f;
		isGrounded = false;
		myRigidbody = GetComponent<Rigidbody2D> ();
		myRigidbody.gravityScale = GRAVITYWHILEFALLING;
		myAnim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Determine if Gimmick is on the ground
		isGrounded = checkIfGrounded ();

		// Calculate new X- and Y-velocity, set gravity, update velocity
		float newXVel = getXVelocity ();
		float newYVel = getYVelocity ();
		myRigidbody.velocity = new Vector3 (newXVel, newYVel, 0f);

		// Make sprite face correct left/right direction
		faceSprite ();

		// Update animator's variables
		myAnim.SetInteger ("xInput", xInput);
		myAnim.SetBool ("Grounded", isGrounded);
	}


	/*
	 * Returns whether Gimmick is on the ground.  To be grounded, Gimmick must be colliding with something in the Ground
	 *   layer directly beneath him, and must have the same (up to FLOATTOLERANCE) velocity as that ground.
	 */
	public bool checkIfGrounded() {
		// Check for collision with Ground-layer object using ground check box
		Collider2D other = Physics2D.OverlapBox (groundCheck.position, groundCheckDim, 0, whatIsGround);
		if (other) {
			// There is ground under Gimmick, check if it has a velocity
			Rigidbody2D otherBody = other.GetComponent<Rigidbody2D> ();
			if (otherBody) {
				// The ground under gimmick has a velocity, check if y-velocities are the same modulo FLOATTOLERANCE
				return (Mathf.Abs (myRigidbody.velocity.y - otherBody.velocity.y) < FLOATTOLERANCE);
			} else {
				// The ground under Gimmick has no velocity, check if our velocity is 0 modulo FLOATTOLERANCE
				return (Mathf.Abs (myRigidbody.velocity.y) < FLOATTOLERANCE);
			}
		} else {
			// No ground under Gimmick
			return false;
		}
	}


	/*
	 * Returns the greater of the two floats.
	 */
	public float max (float first, float second) {
		if (first >= second)
			return first;
		else
			return second;
	}


	/*
	 * Returns the lesser of the two floats.
	 */
	public float min (float first, float second) {
		if (first <= second)
			return first;
		else
			return second;
	}


	/*
	 * Returns the X-velocity Gimmick will have on this frame, based on current situation and input.
	 *    If speeding up, use NORMALACCEL
	 *    If slowing down, use SKIDDECEL
	 *    If grounded with no left/right input, use COASTDECEL
	 *    If airborne with no left/right input, velocity unchanged
	 *    Speed is capped by MAXSPEED
	 */
	public float getXVelocity () {
		float xVel = myRigidbody.velocity.x;

		if (Input.GetAxisRaw ("Horizontal") > 0) {
			// Player inputting "right"
			xInput = RIGHT;
			if (myRigidbody.velocity.x > 0) {
				// Speeding up
				xVel = min (xVel + NORMALACCEL * Time.deltaTime, MAXSPEED);
			} else {
				// Slowing down
				xVel += SKIDDECEL * Time.deltaTime;
			}
		} else if (Input.GetAxisRaw ("Horizontal") < 0) {
			// Player inputting "left"
			xInput = LEFT;
			if (myRigidbody.velocity.x < 0) {
				// Speeding up
				xVel = max (xVel - NORMALACCEL * Time.deltaTime, -MAXSPEED);
			} else {
				// Slowing down
				xVel -= SKIDDECEL * Time.deltaTime;
			}
		} else {
			// No left/right input
			if (isGrounded) {
				// Coasting to a stop
				xInput = NEUTRAL;
				if (xVel >= 0)
					xVel = max (xVel - COASTDECEL * Time.deltaTime, 0f);
				else
					xVel = min (xVel + COASTDECEL * Time.deltaTime, 0f);
			} // else airborne with no left/right input, velocity unchanged
		}

		return xVel;
	}


	/*
	 * Returns the Y-velocity Gimmick will have on this frame, given the current situation and jump input.
	 * Also responsible for setting Gimmick's gravity scale based on whether a jump is being held or not
	 *    When player jumps, Y-velocity is INITIALJUMPSPEED, gravity scale is GRAVITYWHILEJUMMPING
	 *    After a jump, if jump button is still held, gravity scale stays at GRAVITYWHILEJUMPING, when released it becomes GRAVITYWHILEFALLING
	 *       If a jump is held for MAXJUMPDURATION, gravity is automatically set to GRAVITYWHILEFALLING
	 */
	public float getYVelocity () {
		float yVel = myRigidbody.velocity.y;

		if (currentJumpDuration > 0) {
			// Was jumping on previous frame
			if (Input.GetButton ("Jump") && currentJumpDuration < MAXJUMPDURATION) {
				// Still jumping
				currentJumpDuration += Time.deltaTime;
			} else {
				// Ending a jump
				currentJumpDuration = 0;
				myRigidbody.gravityScale = GRAVITYWHILEFALLING;
			}
		} else if (isGrounded && Input.GetButtonDown ("Jump")) {
			// Starting a jump this frame
			yVel = INITIALJUMPSPEED;
			currentJumpDuration += Time.deltaTime;
			myRigidbody.gravityScale = GRAVITYWHILEJUMPING;
		} else {
			// Not jumping on this frame or the previous frame
			yVel = max(yVel, -MAXFALLSPEED);
		}

		return yVel;
	}


	/*
	 * Makes Gimmick face in the same direction as the user's left/right input.
	 * If there is no left/right input, the direction is left unchanged.
	 */
	public void faceSprite () {
		if (xInput == RIGHT)
			transform.localScale = new Vector3 (1f, 1f, 1f);
		else if (xInput == LEFT)
			transform.localScale = new Vector3 (-1f, 1f, 1f);
	}
}
	