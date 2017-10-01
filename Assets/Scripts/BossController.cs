using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour {

	public bool canMove;

	// Current action the boss is taking
	public const int IDLE = 0;
	public const int RUNNING = 1;
	public const int JUMPING = 2;
	public const int FALLING = 3;
	public const int THROWING = 4;
	public const int HURT = 5;
	public const int DIEING = 6;
	public int status;

	private float prevXVel;							// x-velocity on the previous frame

	public List<GameObject> grounds;				// all ground objects the boss is standing on
	public GameObject ground;						// one ground object the boss is standing on, or null if boss is airborne
	public bool isGrounded;							// whether the boss is grounded
	public const float FLOATTOLERANCE = 0.00001f;	// for comparing floating point values for (relative) equality, e.g. to tell whether the boss is standing on a platform.

	private System.Random randObj;					// random number generation

	//Boss AI variables and constants
	public const float HIGHJUMPODDS = 0.5f;			// probability the boss will jump (versus run) when above gimmick.
	public const float LOWJUMPODDS = 0.9f;			// probability the boss will jump (versus run) when below gimmick.

	public double throwTimer;						// timer (sec) that governs when the boss will next attempt to throw a blue shell, counts down to 0.
	public const double THROWTIMERMIN = 2;			// minimum gap between blue shell throw attempts.
	public const double THROWTIMERMAX = 4;			// maximum gap between blue shell throw attempts.

	public double idleTimer;						// timer (sec) that governs the time the boss sits idle before making its next move, counts down to 0.
	public const double IDLETIMERMIN = 0.5;			// minimum value idleTimer can be set to.
	public const double IDLETIMERMAX = 2;			// maximum value idleTimer can be set to.

	public float runDestination;					// x-coordinate boss will try to run to
	public const float RUNSPEED = 10f;				// horizontal running speed (units/sec) of boss
	public const float MINRUNDISTANCE = 2f;			// minimum horizontal distance (units) the boss can attempt to run
	public float MINRUNX;							// minimum x-coordinate the boss can attempt to run to.  Set in Unity editor as this depends on the geometry of the level.
	public float MAXRUNX;							// maximum x-coordinate the boss can attempt to run to.  Set in Unity editor as this depends on the geometry of the level.

	public float jumpTimer;							// timer (sec) for counting down the time between jumping and falling, counts down to 0
	public const float JUMPTIME = 0.5f;				// value to which jumpTimer is set before counting down to 0
	public const float MINJUMPXVEL = -10f;			// minimum x-velocity of a jump
	public const float MAXJUMPXVEL = 10f;			// maximum x-velocity of a jump
	public const float MINJUMPYVEL = 17f;			// minimum y-velocity of a jump
	public const float MAXJUMPYVEL = 25f;			// maximum y-velocity of a jump

	public float throwingTimer;						// timer (sec) for counting down the boss' animation of throwing a blue shell, as well as spawning the blue shell.
	public const float BLUESHELLTIME = 0.25f;		// time that throwingTimer must pass for a blue shell to spawn.
	public const float THROWINGTIME = 0.5f;			// value to which throwingTimer is set before counting down to 0.
	public const float THROWSPEED = 8f;				// initial 2d-speed of a blue shell when thrown.
	public bool shellThrown;						// indicates whether a blue shell has been thrown in this throwing cycle
	public float gravity;							// holds the gravitational constant as defined by Physics.gravity.y, prefetched for efficiency

	public const float HURTSPEED = 12f;				// speed at which the boss moves upward when hurt
	public const float HURTHEIGHT = 15f;			// height above the screen the boss must reach before exiting hurt state
	public int health;

	public const float DEATHTIME = 3f;				// value to which deathTimer is set before counting down to 0
	public float deathTimer;						// timer (sec) for handling the boss' death sequence 
	public float explosionCounter;					// number of explosions to spawn before removing boss from scene
	public const float EXPLOSIONSPERSECOND = 4f;	// number of explosions to spawn per second
	public bool dead;								// indicates whether the boss is dead (no longer present on screen)

	public const float STARPOWERX = 0.75f;			// gets multiplied last into boss' x-velocity when contact with star is made, used for fine-tuning difficulty/feel of fight
	public const float STARPOWERY = 2.5f;			// gets multiplied last into boss' y-velocity when contact with star is made, used for fine-tuning difficulty/feel of fight
	public const float MAXHITYVEL = 25f;			// maximum y-velocity the boss can get when hit by the star, prevents boss from going out of arena

	private Rigidbody2D myRigidBody;
	private BoxCollider2D[] boxes;
	private BoxCollider2D solidBox;					// The solid collision box (used for colliding with ground, walls, star)
	private BoxCollider2D triggerBox;				// The trigger box (used for colliding with gimmick)
	public GameObject gimmick;
	private LevelManager theLevelManager;
	public GameObject deathEffect;
	public GameObject hurtEffect;
	public Animator myAnim;
	public Transform blueShell;
	public Transform blueExplosion;
	public GameObject levelEnd;

	//public AudioSource deathSound;

	// Use this for initialization
	void Start () {
		// Initialize variables to default/starting values
		canMove = false;
		status = IDLE;
		prevXVel = 0f;
		ground = null;
		randObj = new System.Random ();
		throwTimer = (THROWTIMERMAX - THROWTIMERMIN) * randObj.NextDouble () + THROWTIMERMIN;
		idleTimer = 0;
		jumpTimer = 0f;
		throwingTimer = 0f;
		shellThrown = false;
		dead = false;
		explosionCounter = DEATHTIME * EXPLOSIONSPERSECOND;
		health = 3;
		myRigidBody = GetComponent<Rigidbody2D> ();
		gravity = myRigidBody.gravityScale * Physics.gravity.y;

		//Get solidBox and triggerBox
		boxes = GetComponents<BoxCollider2D> ();
		if (boxes [0].isTrigger) {
			solidBox = boxes [1];
			triggerBox = boxes [0];
		} else {
			solidBox = boxes [0];
			triggerBox = boxes [1];
		}
		theLevelManager = FindObjectOfType<LevelManager> ();
		myAnim = GetComponent<Animator> ();
		levelEnd = GameObject.Find ("LevelEnd");
	}
	
	// Update is called once per frame
	void Update () {
		if (canMove) {
			prevXVel = myRigidBody.velocity.x;

			// Update throwTimer, attempt to throw star if throwTimer expired (need to be not hurt or dying)
			if (throwTimer < 0) {
				if (status != HURT && status != DIEING)
					setStatus (THROWING);
				throwTimer = (THROWTIMERMAX - THROWTIMERMIN) * randObj.NextDouble () + THROWTIMERMIN;
			} else
				throwTimer -= Time.deltaTime;

			// Pass to status handler
			if (status == IDLE)
				handleIdle ();
			else if (status == RUNNING)
				handleRunning ();
			else if (status == JUMPING)
				handleJumping ();
			else if (status == FALLING)
				handleFalling ();
			else if (status == THROWING)
				handleThrowing ();
			else if (status == HURT)
				handleHurt ();
			else if (status == DIEING)
				handleDieing ();

			// Update sprite and animation variables
			faceSprite ();
			myAnim.SetInteger ("Status", status);
		}
	}

	// While idle, when idleTimer expires switch to run or jump.
	public void handleIdle() {
		// Update idleTimer, switch to run/jump if it expires
		if (idleTimer < 0) {
			// exit idle to either run or jump, with odds of each governed by HIGHJUMPODDS and LOWJUMPODDS
			double runJumpDecider = randObj.NextDouble(); 
			if ((transform.position.y - 2f >= gimmick.transform.position.y && runJumpDecider < HIGHJUMPODDS) || (transform.position.y - 2f < gimmick.transform.position.y && runJumpDecider < LOWJUMPODDS)) {
				// jumping
				myRigidBody.velocity = getJumpVelocity ();
				transform.position = new Vector3 (transform.position.x, transform.position.y + 0.01f, 0f);
				setStatus (JUMPING);
			} else {
				// running
				setRunDestination ();
				setStatus (RUNNING);
			}
		} else {
			idleTimer -= Time.deltaTime;
		}
	}

	// Runs at RUNSPEED towards runDestination until reached, after while boss becomes idle.
	public void handleRunning() {
		if (myRigidBody.velocity.x > 0) {
			myRigidBody.velocity = new Vector3 (RUNSPEED, 0f, 0f);
			if (transform.position.x >= runDestination)
				setStatus (IDLE);
		} else {
			myRigidBody.velocity = new Vector3 (-RUNSPEED, 0f, 0f);
			if (transform.position.x <= runDestination)
				setStatus (IDLE);
		}
	}

	// Handles transition between jumping and either falling or idle
	public void handleJumping() {
		// idle if landed
		if (isGrounded)
			setStatus (IDLE);
		// update jumpTimer, falling if expires
		else if (jumpTimer < 0)
			setStatus (FALLING);
		else
			jumpTimer -= Time.deltaTime;
	}

	// Handles transition between falling and idle
	public void handleFalling() {
		// idle if landed
		if (isGrounded)
			setStatus (IDLE);
	}

	// Handles throwing of a blue shell, and transition to falling/idle
	public void handleThrowing() {
		// update throwing timer, change status if expired
		if (throwingTimer < 0) {
			// idle if grounded, falling if airborne
			if (isGrounded)
				setStatus (IDLE);
			else
				setStatus (FALLING);
		} else if (throwingTimer < BLUESHELLTIME && !shellThrown) {
			// throwingtimer just passed BLUESHELLTIME, throw blue shell
			throwBlueShell ();
			shellThrown = true;
			throwingTimer -= Time.deltaTime;
		} else
			throwingTimer -= Time.deltaTime;
	}

	// Handles boss getting hurt, transition to falling
	public void handleHurt() {
		// move upwards at HURTSPEED until high enough above camera, then re-enable hitbox and fall down above gimmick
		if (transform.position.y - Camera.main.transform.position.y > HURTHEIGHT) {
			solidBox.enabled = true;
			transform.position = new Vector3 (gimmick.transform.position.x, transform.position.y - 1, 0f);
			myRigidBody.velocity = new Vector3 (0f, 0f, 0f);
			setStatus (FALLING);
		} else {
			myRigidBody.velocity = new Vector3 (0f, HURTSPEED, 0f);
		}
	}

	// Handles the dying animation of the boss
	public void handleDieing() {
		if (deathTimer < 0 && !dead) {
			// deathTimer just expired, do death effects and remove boss from scene
			levelEnd.GetComponent<SpriteRenderer>().enabled = true;
			dead = true;
            //deathSound.Play();
            AudioManager.instance.PlaySound2D("Boss Death");
            Instantiate (deathEffect, transform.position, transform.rotation);
			Destroy (gameObject);
		} else {
			deathTimer -= Time.deltaTime;
			// Create explosionCounter explosions at the rate of EXPLOSIONSPERSECOND
			if (deathTimer * EXPLOSIONSPERSECOND < explosionCounter && explosionCounter > 0) {
				explosionCounter--;

				// position of explosion is at random spot on boss' hitbox
				float expX = transform.position.x + ((float)randObj.NextDouble ()) * 1.5f - 0.75f;
				float expY = transform.position.y + ((float)randObj.NextDouble ()) * 2.625f - 1.3125f;

				Transform blueExplosionClone = (Transform)UnityEngine.Object.Instantiate (blueExplosion, new Vector3 (expX, expY, 0f) , Quaternion.identity);
				AudioManager.instance.PlaySound2D("Blue Shell"); 
			}
		}
		
	}

	// Faces boss' sprite in correct direction.  If running, faces forward.  Otherwise faces towards gimmick.
	public void faceSprite() {
		if (status == RUNNING) {
			if (myRigidBody.velocity.x > 0)
				transform.localScale = new Vector3 (1f, 1f, 1f);
			else
				transform.localScale = new Vector3 (-1f, 1f, 1f);
		} else {
			if (transform.position.x < gimmick.transform.position.x)
				transform.localScale = new Vector3 (1f, 1f, 1f);
			else
				transform.localScale = new Vector3 (-1f, 1f, 1f);
		}
	}

	// Handles the setting of all timers and other variables when the status is changed.
	public void setStatus(int newStatus) {
		// zero-out everything
		idleTimer = 0;
		jumpTimer = 0f;
		throwingTimer = 0f;
		shellThrown = false;
		deathTimer = 0f;

		// if becoming idle, set idleTimer
		if (newStatus == IDLE)
			idleTimer = (IDLETIMERMAX - IDLETIMERMIN) * randObj.NextDouble () + IDLETIMERMIN;
		// if jumping, set jumpTimer
		else if (newStatus == JUMPING)
			jumpTimer = JUMPTIME;
		// if throwing, set throwingTimer
		else if (newStatus == THROWING) {
			throwingTimer = THROWINGTIME;
			// if changing from running to throwing, zero out velocity to prevent sliding while throwing
			if (status == RUNNING) {
				myRigidBody.velocity = new Vector3 (0f, 0f, 0f);
			}
		}
		// if becoming hurt, disable collision box so we can't interrupt the hurt animation
		else if (newStatus == HURT) {
			Instantiate (hurtEffect, transform.position, transform.rotation);
			solidBox.enabled = false;
		}
		// if dying , set deathTimer
		else if (newStatus == DIEING) {
			deathTimer = DEATHTIME;
		}
		status = newStatus;
	}

	// Handles the throwing of a blue shell.  Calculates the blue shell's initial position and velocity, and spawns it.
	public void throwBlueShell() {
		float gimmX = gimmick.transform.position.x;
		float gimmY = gimmick.transform.position.y;

		// calculate blue shell's initial position
		float handX;
		if (transform.localScale.x > 0)
			handX = transform.position.x + 1f;
		else
			handX = transform.position.x - 1f;
		float handY = transform.position.y - 0.5f;
		Vector3 handPos = new Vector3 (handX, handY, 0f);

		// calculate initial velocity so that blue shell travels along a parabolic arc to Gimmick (TODO: this isn't working accurately)
		float a = gravity * (gimmX - handX) * (gimmX - handX) / (2f * THROWSPEED * THROWSPEED);
		Vector2 tangents = solveQuadratic (a, THROWSPEED, a - (gimmY - handY));
		float xVel, yVel;
		if (tangents.x == 0 && tangents.y == 0) {
			// gimmick out of range, throw at 45-degree upward angle toward gimmick
			xVel = transform.localScale.x * THROWSPEED * 1.4f;
			yVel = THROWSPEED * 1.4f;
		} else {
			// gimmick in range.  Choose the parabolic arc with initial upward velocity
			float tangent;
			if (transform.localScale.x > 0)
				tangent = Mathf.Max (tangents.x, tangents.y);
			else
				tangent = Mathf.Min (tangents.x, tangents.y);
			float hyp = Mathf.Sqrt (tangent * tangent + 1);
			xVel = transform.localScale.x * THROWSPEED / hyp;
			yVel = tangent * THROWSPEED / hyp;
		}

		// spawn blue shell
		Transform blueShellClone = (Transform)UnityEngine.Object.Instantiate (blueShell, handPos, Quaternion.identity);
		blueShellClone.GetComponent<Rigidbody2D> ().velocity = new Vector3 (xVel, yVel, 0f);

		// set blue shell's target to be 1 unit beyond gimmick horizontally, 1 unit below gimmick vertically.  Blue shell explodes when targetX or targetY is surpassed.
		if (transform.localScale.x > 0)
			blueShellClone.GetComponent<BlueShellController> ().targetX = gimmX + 1f;
		else
			blueShellClone.GetComponent<BlueShellController> ().targetX = gimmX - 1f;
		blueShellClone.GetComponent<BlueShellController> ().targetY = gimmY - 1f;
	}

	// Returns the two solutions to the quadratic ax^2 + bx + c = 0, or (0,0) if no real solution exists.
	public Vector2 solveQuadratic (float a, float b, float c) {
		float disc = b * b - 4 * a * c;
		if (disc > 0)
			return new Vector2 ((-b + Mathf.Sqrt (disc)) / (a * 2), (-b - Mathf.Sqrt (disc)) / (a * 2));
		else
			return new Vector2 (0, 0);
	}

	// Modifies the boss' velocity when touching the star.  Hurts the boss when touching the spikes.  Updates the grounds/ground/isGrounded variables when touching ground.
	void OnCollisionEnter2D(Collision2D other) {
		Debug.Log ("Tag: " + other.transform.tag);
		if (other.transform.tag == "Star" && status != DIEING && status != HURT) {
			Debug.Log ("Star Hit");
			Vector3 starVel = other.gameObject.GetComponent<Rigidbody2D> ().velocity;

			// Push boss in direction corresponding to vector between star and boss centers, with strength taken from projecting star velocity onto this vector
			// TODO: why am I using gimmick's position here and not the star's?
			Vector3 pushVecRaw = getProjection (starVel, gimmick.transform.position - transform.position);

			// Modify boss velocity by STARPOWERX and STARPOWERY for fine-tuning, since just above velocity was too difficult to play with.  Cap yVel with MAXHITYVEL.
			float newXVel = pushVecRaw.x * STARPOWERX;
			float newYVel = Mathf.Min (pushVecRaw.y * STARPOWERY, MAXHITYVEL);
			myRigidBody.velocity = new Vector3 (newXVel, newYVel, 0f);
			setStatus (FALLING);

			// Destroy star, disconnect gimmick from star if he was riding it
			other.gameObject.SetActive (false);
			if (gimmick.GetComponent<GimmickController> ().joint) {
				Destroy (gimmick.GetComponent<SliderJoint2D> ());
				gimmick.GetComponent<GimmickController> ().joint = null;
			}
		} else if (other.transform.tag == "BossSpikes") {
			// Hit spikes, transition to death or hurt
			health--;
			if (health == 0)
				setStatus (DIEING);
			else
				setStatus (HURT);
		} else if (other.transform.tag == "Ground" || other.transform.tag == "BossWall") {
			if (other.contacts.Length >= 2) {
				// When two box colliders collide, contacts (normally?) has the two endpoints of the "line segment" where they intersect
				float y1 = other.contacts [0].point.y;
				float y2 = other.contacts [1].point.y;
				float groundHeight = other.gameObject.transform.position.y + other.collider.offset.y; // Height of the center of ground's collider box
				float myHeight = transform.position.y + other.otherCollider.offset.y; // Height of center of Gimmick's collider box
				if ((Mathf.Abs (y1 - y2) < FLOATTOLERANCE) && groundHeight < myHeight && myRigidBody.velocity.y <= FLOATTOLERANCE) {
					// Touched top or bottom of ground's box, and are above box (so touched top of box)
					//groundChanged = true;
					isGrounded = true;
					// Add ground object to list if it isn't already there, update ground
					if (!grounds.Find (g => UnityEngine.Object.ReferenceEquals (g.gameObject, other.gameObject))) {
						grounds.Add (other.gameObject);
						ground = grounds [0];
					}
				} else if (other.transform.tag == "BossWall" && Mathf.Abs (y1 - y2) >= FLOATTOLERANCE) {
					//Touched side of bosswall, rebound
					myRigidBody.velocity = new Vector3 (-prevXVel, myRigidBody.velocity.y, 0f);
				}
			}
		}
	}

	// Updates grounds/ground variables for this frame
	void OnCollisionStay2D(Collision2D other) {
		if (other.transform.tag == "Ground" || other.transform.tag == "BossWall") {
			if (other.contacts.Length >= 2) {
				// When two box colliders collide, contacts (normally?) has the two endpoints of the "line segment" where they intersect
				float y1 = other.contacts [0].point.y;
				float y2 = other.contacts [1].point.y;
				float groundHeight = other.gameObject.transform.position.y + other.collider.offset.y; // Height of the center of ground's collider box
				float myHeight = transform.position.y + other.otherCollider.offset.y; // Height of center of Gimmick's collider box
				if ((Mathf.Abs (y1 - y2) < FLOATTOLERANCE) && groundHeight < myHeight && myRigidBody.velocity.y <= FLOATTOLERANCE) {
					// Touched top or bottom of ground's box, and are above box (so touched top of box)
					//groundChanged = true;
					isGrounded = true;
					// Add ground object to list if it isn't already there, update ground
					if (!grounds.Find (g => UnityEngine.Object.ReferenceEquals (g.gameObject, other.gameObject))) {
						grounds.Add (other.gameObject);
						ground = grounds [0];
					}
				} 
			}
		}
	}

	// Updates ground/grounds variables, switch to falling if airborne
	void OnCollisionExit2D(Collision2D other) {
		// Check if leaving a ground object
		if (grounds.Find(g => UnityEngine.Object.ReferenceEquals (g.gameObject, other.gameObject))) {
			// Remove this ground from the list, update ground
			//groundChanged = true;
			grounds.RemoveAll(g => UnityEngine.Object.ReferenceEquals (g.gameObject, other.gameObject));
			if (grounds.Count > 0)
				ground = grounds [0];
			else {
				isGrounded = false;
				ground = null;
				if (status != JUMPING)
					setStatus (FALLING);
			}
		}
	}

	// Returns the orthogonal projection of source onto target.
	public Vector3 getProjection (Vector3 source, Vector3 target) {
		float mag = (target.x * source.x + target.y * source.y) / (target.x * target.x + target.y * target.y);
		return new Vector3 (target.x * mag, target.y * mag, 0f);
	}

	// Returns a random velocity with x- and y-velocities bounded by their respective min and max constants.
	// If the boss is far enough to the right, only a negative x-velocity is permitted.
	public Vector3 getJumpVelocity() {
		float xVel;
		// If boss far enough to the right, use negative x-velocity, to avoid suicide
		if (transform.position.x > (3 * MINRUNX + MAXRUNX) / 4)
			xVel = (float)(randObj.NextDouble () * MINJUMPXVEL);
		else
			xVel = (float)(randObj.NextDouble () * (MAXJUMPXVEL - MINJUMPXVEL) + MINJUMPXVEL);
		float yVel = (float)(randObj.NextDouble () * (MAXJUMPYVEL - MINJUMPYVEL) + MINJUMPYVEL);
		return new Vector3 (xVel, yVel, 0f);
	}

	// Sets the boss' run destination
	public void setRunDestination() {
		int runDirection;
		float runDistance;
		// If within MINRUNDISTANCE of the edge of the arena, run away from it
		if (transform.position.x > MAXRUNX - MINRUNDISTANCE)
			runDirection = -1;
		else if (transform.position.x < MINRUNX + MINRUNDISTANCE)
			runDirection = 1;
		// Else randomly choose to run left or right, with bias given towards running toward the center of the arena.
		else {
			double leftRightDecider = randObj.NextDouble () * (MAXRUNX - MINRUNX) + MINRUNX;
			if (transform.position.x > leftRightDecider)
				runDirection = -1;
			else
				runDirection = 1;
		}

		// Choose a random distance to run (above MINRUNDISTANCE, below distance to edge of arena), set velocity
		if (runDirection == -1) {
			runDistance = (float)(randObj.NextDouble () * (transform.position.x - MINRUNX - MINRUNDISTANCE) + MINRUNDISTANCE);
			runDestination = transform.position.x - runDistance;
			myRigidBody.velocity = new Vector3 (-RUNSPEED, 0f, 0f);
		} else {
			runDistance = (float)(randObj.NextDouble () * (MAXRUNX - transform.position.x - MINRUNDISTANCE) + MINRUNDISTANCE);
			runDestination = transform.position.x + runDistance;
			myRigidBody.velocity = new Vector3 (RUNSPEED, 0f, 0f);
		}
			
	}

	void OnBecameVisible()
	{
		canMove = true;
	}
}
