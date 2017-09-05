using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour {
	public AudioSource creditsMusic;

	public GameObject background;			// The upper half (city) of the scrolling background
	public GameObject ground; 				// The lower half (street) of the scrolling background
	public GameObject gimmickOnSkateboard;
	public GameObject canvas;
	public Rigidbody2D myRigidbody;

	// Constants for moving ground
	public const float GROUNDMOVESPEED = 6f;	// Speed at which ground moves right to left
	public const float GROUNDMINX = -3f;		// Position of ground when position should loop back to start
	public const float GROUNDLOOPSIZE = 4f;		// Distance ground should move to the right when loop occurs

	public Sprite sprite1, sprite2, sprite3, sprite4;
	public SpriteRenderer renderer;

	// Constants for moving background
	public const float BACKMOVESPEED = .35f;	// Speed at which background moves right to left
	public int currentSprite;					// Index of the current background image used, ranges from 1 to 4
	public const float SPRITE2TRANS = -3.6875f;	// Position of background when transitioning from image 1 to 2
	public const float SPRITE2DIFF = 11.8125f;	// Distance to move background to the right when transitioning from image 1 to 2
	public const float SPRITE3TRANS = -5.6875f;	// Position of background when transitioning from image 2 to 3
	public const float SPRITE3DIFF= 9.5625f;	// Distance to move background to the right when transitioning from image 2 to 3
	public const float SPRITE4TRANS = -2.1875f;	// Position of background when transitioning from image 3 to 4
	public const float SPRITE4DIFF = 9.8125f;	// Distance to move background to the right when transitioning from image 3 to 4

	// Credits-related variables
	public float creditsTimer;					// Timer that governs when each text box is drawn
	public int nextLine;						// Index in credits array of next line to use
	public const float TIMEBETWEENLINES = 0.6f;	// Time between normal text box spawns
	public const float WAITFORNEWBLOCK = 0.9f;	// Extra time to wait before a new block of credits, i.e. before a line starting with '-'
	public const float TIMETOTHEENDTEXT = 2.5f;	// Extra time to wait before drawing the last text box containing "The End"
	public float timeToNextLine;				// Full time to wait until next text box spawns (by adding some of above 3 constants together)
	public string[] credits;					// The actual strings of credits data
	public int creditsSize;						// Size of credits array
	public GameObject textBox;

	// Global timing variables
	public float globalTimer;					// Timer that counts up from scene loading
	public const float TIMETOSCROLL = 110f;		// Total time that backgrounds should scroll
	public const float TIMETOEXIT = 115f;		// Total time until post-credits scene loads
	public bool gimmickMoved;					// Indicates whether Gimmick has transitioned from stationary (rel to screen) to moving

	public string postCreditsSceneName;			// Scene to load after credits sequence

	// Use this for initialization
	void Start () {
		creditsMusic = GameObject.Find ("Credits Music").GetComponent<AudioSource> ();
		creditsMusic.volume *= (PlayerPrefs.GetFloat("Master Vol") * PlayerPrefs.GetFloat("Music Vol"));
		creditsMusic.Play ();
		myRigidbody = gimmickOnSkateboard.GetComponent<Rigidbody2D> ();
		renderer = background.GetComponent<SpriteRenderer> ();
		renderer.sprite = sprite1;
		currentSprite = 1;
		nextLine = 0;
		timeToNextLine = TIMEBETWEENLINES;
		creditsTimer = TIMEBETWEENLINES;
		credits = new string[103] { "CONGRATULATIONS!", 
			"You've defeated the vile Squirrelbot", 
			"and restored peace to the world.",
			"Consider yourself a hero.",
			"----------CREDITS----------", 
			"-------APPLICATION DEVELOPMENT-------",
			"Robert McGuigan",
			"Monica Pineda",
			"Bilal Saleem",
			"-------CORE GAMEPLAY CONCEPTS-------",
			"Sunsoft (Mr. Gimmick)",
			"-------SRITES AND BACKGROUNDS-------",
			"---ALPHADREAM, GOOD-FEEL---",
			"Crab (Mario & Luigi: Dream Team)",
			"---ANGEVON'S RO SPRITES ARCHIVE---",
			"Kraken Tentacle",
			"---BEN BAKKER---",
			"Cave Exterior",
			"---CAPCOM---",
			"Explosions (Bionic Commando)",
			"Credits Background (Final Fight)",
			"Conveyor Belt (Mega Man 2)",
			"Various Stage Elements (Mega Man 6)",
			"---EMROX (NEWGROUNDS)---",
			"Title Art",
			"---EVERPLAY INTERACTIVE---",
			"Forest Level Background (Spellsword)",
			"---GAME GARDEN---",
			"Palm Tree (Fairy Farm)",
			"---HAL---",
			"Cloud (Kirby Super Star)",
			"---LJN---",
			"Skateboard (Town & Country Surf Design: Wood & Water Rage)",
			"---MFO WIKI---",
			"Jumping Seagull",
			"---NAMCO---",
			"Turret (Xevious)",
			"---NINTENDO---",
			"Blue Shell (Mario Kart Super Circuit)",
			"City Street (Mike Tyson's Punch-Out!!)",
			"Bullet (StarTropics)",
			"Mushroom (Super Mario All-Stars)",
			"Koopa Troopa, Fire Bar Concept (Super Mario Bros)",
			"Spiky Spikes (Super Mario Bros 2)",
			"Various Stage Elements (Super Mario Bros 3)",
			"---OPENGAMEART---",
			"Various Stage Elements (Plee the Bear)",
			"Water",
			"---PRIVATIA, PUMPCHI STUDIO---",
			"Flying Fish (Trickster Online Revolution)",
			"---RARE---",
			"Blue Robot (Battletoads and Double Dragon)",
			"Flag (R.C. Pro-Am)",
			"---RYKY (DEVIANTART)---",
			"Water Droplets",
			"---SEGA---",
			"Fireball (Ghostbusters)",
			"Seaside Level Background (Sonic Lost World)",
			"Factory Level Background, Various Stage Elements (Sonic the Hedgehog)",
			"---SIPUT SCUBA---",
			"Flying Seagull",
			"---SUNSOFT---",
			"Gimmick, Star, Title Art (Mr. Gimmick)",
			"SquirrelBot (Zero the Kamikaze Squirrel)",
			"---TRIBUTE GAMES---",
			"Bear (Curses N' Chaos)",
			"---UDEMY---",
			"Launcher, Moving Platform, Spikes, Stalactite, Coin, Heart, Fish, Various Stage Elements:",
			"\"Learn To Code By Making a 2D Platformer\"",
			"---VIZOR INTERACTIVE---",
			"Tree (Zombie Island)",
			"-------MUSIC-------",
			"---RARE---",
			"Cave Level, Victory (Battletoads)",
			"---SQUARE---",
			"Credits (Rad Racer II)",
			"---SUNSOFT---",
			"Boss Theme (Batman: The Video Game)",
			"Factory Level (Journey to Silius)",
			"Forest Level (Ufouria: The Saga)",
			"---TREVOR LENTZ---",
			"Seaside Level (Guinea Pig Hero)",
			"-------SOUND EFFECTS-------",
			"---COMPILE---",
			"Explosions and Gunfire (Gun-Nac)",
			"---KONAMI---",
			"Explosions (Contra)",
			"---NINTENDO---",
			"Jump, Health Get (Super Mario World)",
			"---SUNSOFT---",
			"Star Impact (Ufouria: The Saga)",
			"---UDEMY---",
			"Hurt, Explosions, Coin Get",
			"-------SPECIAL THANKS-------",
			"---UDEMY---",
			"Their \"Learn to code by making a 2D Platformer (Unity)\" course really got us going!",
			"---WIIGUY'S 8BITSTEREO---",
			"For the awesome stereo remixes of classic 8-bit tunes!",
			"---STIMPY789---",
			"SquirrelBot Concept!",
			"---YOU---",
			"Thanks for playing!",
			"THE END"
		};
		creditsSize = credits.Length;
		globalTimer = 0f;
		gimmickMoved = false;
	}
	
	// Update is called once per frame
	void Update () {
		globalTimer += Time.deltaTime;
		if (globalTimer < TIMETOSCROLL) { // Still scrolling the background
			moveGround ();
			moveBackground ();
		} else if (!gimmickMoved) { // Transitioning to non-moving background and moving Gimmick
			moveGimmick ();
			gimmickMoved = true;
		} else if (globalTimer > TIMETOEXIT) { // Loading the next scene
			SceneManager.LoadScene (postCreditsSceneName);
		} // Else still in moving-Gimmick phase, no other action needed
		updateCredits ();
	}

	// Moves the ground (street, lower half) at a constant speed to the left, looping back to the right to simulate an infinitely-long street
	public void moveGround () {
		float newGroundX = ground.transform.position.x - GROUNDMOVESPEED * Time.deltaTime;
		if (newGroundX < GROUNDMINX) // Time to loop back to the right
			newGroundX += GROUNDLOOPSIZE;
		ground.transform.position = new Vector3 (newGroundX, ground.transform.position.y, 0f);
	}

	// Moves the background (city, upper half) at a constant speed to the left, replacing the background image with 1 of 4 variations to simulate a night-day transition.
	public void moveBackground() {
		float newBackX = background.transform.position.x - BACKMOVESPEED * Time.deltaTime;
		if (currentSprite == 1 && newBackX < SPRITE2TRANS) { // Transitioning from image 1 to 2
			newBackX += SPRITE2DIFF;
			renderer.sprite = sprite2;
			currentSprite++;
		} else if (currentSprite == 2 && newBackX < SPRITE3TRANS) { // Transitioning from image 2 to 3
			newBackX += SPRITE3DIFF;
			renderer.sprite = sprite3;
			currentSprite++;
		} else if (currentSprite == 3 && newBackX < SPRITE4TRANS) { // Transitioning from image 3 to 4
			newBackX += SPRITE4DIFF;
			renderer.sprite = sprite4;
			currentSprite++;
		}
		background.transform.position = new Vector3 (newBackX, background.transform.position.y, 0f);
	}

	// Causes Gimmick to start moving to the right (relative to the screen)
	public void moveGimmick () {
		myRigidbody.velocity = new Vector3 (GROUNDMOVESPEED, 0f, 0f);
	}

	// Periodically creates a textbox with the next line of credits.  The placement, movement and destruction of the textbox is handled by CreditsTextController.
	public void updateCredits () {
		if (nextLine < creditsSize) { // More textboxes to draw
			creditsTimer += Time.deltaTime;
			if (creditsTimer > timeToNextLine) { // Time to draw the next textbox
				creditsTimer -= timeToNextLine;
				GameObject textBoxClone = UnityEngine.Object.Instantiate (textBox, canvas.transform);
				textBoxClone.transform.Find ("CreditsText").gameObject.GetComponent<UnityEngine.UI.Text> ().text = credits [nextLine];
				nextLine++;

				// Calculate the time to draw the next textbox
				if (nextLine < creditsSize) {
					if (credits [nextLine] [0] == '-') // Next textbox is start of a block, wait extra
						timeToNextLine = TIMEBETWEENLINES + WAITFORNEWBLOCK;
					else if (nextLine != creditsSize - 1) // Normal wait time
						timeToNextLine = TIMEBETWEENLINES;
					else // Next textbox is "The End", wait extra
						timeToNextLine = TIMETOTHEENDTEXT;
				}
			}
		}
	}
}
