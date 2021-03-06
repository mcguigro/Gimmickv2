﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/* when you add this to the scene add canvas from the prefabs folder and drag from under the canvas->heart holder move heart  into heart 1 heart(1) into heart 2 and heart (2) into heart 3 
 ** Also drag in canvas-> pointText into the PointText in the level manager
 */
public class LevelManager : MonoBehaviour
{

    public float waitToRespawn;//time the game waits to respawn
    public GimmickController Gimmick;//gives access to Gimmick
    public Text PointText;// UI points element
    public int currentScore = 0;

    //health sprites system
    public Image heart1;
    public Image heart2;
    public Image heart3;
	public Image heart4; // bonus heart
	public Image heart5; // bonus heart
	public Image heart6; // bonus heart
	public Image heart7; // bonus heart 
    public Sprite heartFull;
    public Sprite heartHalf;
    public Sprite heartEmpty;
	public Sprite invisibleItem; 

    public int maxHealth;//max health the player has
    public int healthCount;//health count how much health Gimmick currently has
    public bool Invincible;// has he just been hit?
	public float flashTimer;// time for which Gimmick will be flashing, or <= 0 if not flashing
	private SpriteRenderer spriteRenderer;// Gimmick's sprite renderer
    private bool respawning;// is he currently respawning?
    public GameObject deathsplosion;// particle effect when Gimmick dies
    public GameObject gameOverScreen;//ability to activeate the game over screen 
    //public AudioClip coinSound;
    //public AudioClip heartSound;
   // public AudioClip levelMusic;
    public AudioSource GameOverMusic;
    public AudioSource MainMusic;
	public AudioSource bossMusic;
	public GameObject levelEnd;



    private HighScore theHighScore;

	public string maxHealthKey = "MaxHealth";			//+ key used in PlayerPrefs to store max health of Gimmick

	// 2D Array of spawn locations for Gimmick.  Each row corresponds to a level (levels are in numerical order), first entry in
	//    a row is the spawn point when first entering a level; additional entries are respawn points after triggering a checkpoint with the matching progress index.
	public static readonly Vector3[][] restartLocations = new [] {new [] { new Vector3 (-30, 33, 0), new Vector3 (-30, 33, 0) },
		new [] { new Vector3 (-35, -21, 0), new Vector3 (-35, -21, 0) }, 
		new [] { new Vector3 (-18.5f, 4.5f, 0), new Vector3 (-18.5f, 4.5f, 0) }, 
		new [] { new Vector3 (-25.3f, -12.5f, 0), new Vector3 (382, -6, 0) }
	};

    // Use this for initialization
    void Start()
    {
		// Initialize references to other objects
        Gimmick = FindObjectOfType<GimmickController>();
		levelEnd = GameObject.Find ("LevelEnd");

		// Disable level end of final level until boss is defeated
		if (SceneManager.GetActiveScene ().name == "Forest Level")
			levelEnd.GetComponent<SpriteRenderer>().enabled = false;

        AudioManager.instance.PlayMusic(MainMusic);

		// Initialize various other variables
        if (PlayerPrefs.HasKey(maxHealthKey)){
			maxHealth = PlayerPrefs.GetInt (maxHealthKey);
		}
		healthCount = maxHealth;
		flashTimer = 0f;
		spriteRenderer = Gimmick.gameObject.GetComponent<SpriteRenderer> ();
        PointText.text = "Current Score: " + currentScore;
		theHighScore = FindObjectOfType<HighScore> ();

		// Display correct hearts
		UpdateHeartMeter ();

		// Set starting location
		int levelIndex = getLevelIndex ();
		if (levelIndex > -1)
			Gimmick.transform.position = restartLocations[levelIndex][PlayerPrefs.GetInt("Level Progress")];
    }

    // Update is called once per frame
    void Update()
    {
		// Handle the flashing of Gimmick's sprite when invincible
		updateFlashEffect ();
    }
    //This adds points to the Current Score 
    public void addPoints(int pointsToAdd)
    {
       
        currentScore += pointsToAdd;
        PointText.text = "Current Score: " + currentScore;
    }
    //Starts Repawinging coroutine if gimmick dies
    public void Respawn()
    {

        StartCoroutine("RespawnCo");
    }
    // Deactivates Gimmick from the screen, Shows the particle effect of his death and loads the game over screen
    public IEnumerator RespawnCo()
    {
		// Remove Gimmick and create death-splosion
        Gimmick.gameObject.SetActive(false);
        AudioManager.instance.PlaySound2D("Explosion");
        Instantiate(deathsplosion, Gimmick.transform.position, Gimmick.transform.rotation);
        yield return new WaitForSeconds(waitToRespawn);

		// Freeze action, bring up game over screen, save high score, change music
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
		theHighScore.SaveScore ();   // save the score of the player
		bossMusic.Stop();
        AudioManager.instance.ChangeMusic(GameOverMusic, 5);
       // levelMusic.Stop();
        //GameOverMusic.Play();
    }
    //If Gimmick runs into an enemy or danger and takes damage
    public void HurtPlayer(int damageToTake)
    {
        //If Gimmick isn't Invincible decrease health 
        if (!Invincible)
        {
			// Decrease health, update heart meter, kill Gimmick if health is <= 0
            healthCount -= damageToTake;
            UpdateHeartMeter();
            //Gimmick.hurtSound.Play();
            // AudioManager.instance.PlaySound(Gimmick.hurtSound, transform.position);
            //AudioManager.instance.PlaySound("Hurt", transform.position);
            AudioManager.instance.PlaySound2D("Hurt");
            if (healthCount <= 0)
				Respawn ();
			Invincible = true;
			StartCoroutine ("HurtPlayerCo");
        }
    }

	// Handles making Gimmick invincible for flashTimer seconds.
	//PRECONDITION: flashTimer must be set with the time to be invincible before calling this; this happens in HurtPlayer.
	private IEnumerator HurtPlayerCo(){
		// make player immune to damage for a duration
		yield return new WaitForSeconds (flashTimer);
		Invincible = false;
	}
		
    //This is for Items that give Gimmick health
    public void GiveHealth(int healthToGive)
    {
       
        healthCount += healthToGive;
        //heartSound.Play();
        // AudioManager.instance.PlaySound(heartSound, transform.position);
        //AudioManager.instance.PlaySound("1-UP", transform.position);
        AudioManager.instance.PlaySound2D("1-UP");
        
        if (healthCount > maxHealth)
        {
            healthCount = maxHealth;
        }
        UpdateHeartMeter();

    }
    //This updates the heart images that represent health on the players screen
    public void UpdateHeartMeter()
    {
		// TO-DO: Un-switch-ify this and the below functions at some point.
		if (PlayerPrefs.HasKey (maxHealthKey)) {
			switch (PlayerPrefs.GetInt (maxHealthKey)) {
			case 6:
				sixHealth ();
                maxHealth = 6;
				return;
			case 8:
				eightHealth ();
                maxHealth = 8;
				return;
			case 10:
				tenHealth ();
                maxHealth = 10;
                return;
			case 12:
				twelveHealth ();
                maxHealth = 12;
				return;
			case 14:
				fourteenHealth ();
                maxHealth = 14;
                return;
			default:
				sixHealth ();
				Debug.Log ("Default in UpdateHeartMeter() being called");
				return;
			}
		} else {
			sixHealth ();
			Debug.Log ("maxHealthKey not found");
		}
    }

	private void sixHealth(){
		switch (healthCount)
		{
		case 6:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 5:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartHalf;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 4:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 3:
			heart1.sprite = heartFull;
			heart2.sprite = heartHalf;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 2:
			heart1.sprite = heartFull;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 1:
			heart1.sprite = heartHalf;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 0:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		default:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = invisibleItem;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		}
	}

	private void eightHealth(){
		switch (healthCount) {
		case 8:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 7:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartHalf;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 6:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 5:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartHalf;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 4:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 3:
			heart1.sprite = heartFull;
			heart2.sprite = heartHalf;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 2:
			heart1.sprite = heartFull;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 1:
			heart1.sprite = heartHalf;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 0:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		default:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = invisibleItem;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;

		}
	}

	private void tenHealth(){
		switch (healthCount) {
		case 10:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 9:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartHalf;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 8:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 7:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartHalf;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 6:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 5: 
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartHalf;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 4:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 3:
			heart1.sprite = heartFull;
			heart2.sprite = heartHalf;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 2:
			heart1.sprite = heartFull;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 1:
			heart1.sprite = heartHalf;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		case 0:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		default:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = invisibleItem;
			heart7.sprite = invisibleItem;
			return;
		}
	}

	private void twelveHealth(){
		switch (healthCount) {
		case 12:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartFull;
			heart7.sprite = invisibleItem;
			return;
		case 11:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartHalf;
			heart7.sprite = invisibleItem;
			return;
		case 10:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 9:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartHalf;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 8:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 7:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartHalf;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 6:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 5:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartHalf;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 4:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 3:
			heart1.sprite = heartFull;
			heart2.sprite = heartHalf;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 2:
			heart1.sprite = heartFull;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 1:
			heart1.sprite = heartHalf;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		case 0:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		default:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = invisibleItem;
			return;
		}
	}

	private void fourteenHealth(){
		switch (healthCount) {
		case 14:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartFull;
			heart7.sprite = heartFull;
			return;
		case 13:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartFull;
			heart7.sprite = heartHalf;
			return;
		case 12: 
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartFull;
			heart7.sprite = heartEmpty;
			return;
		case 11:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartHalf;
			heart7.sprite = heartEmpty;
			return;
		case 10:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartFull;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 9:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartHalf;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 8:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartFull;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 7:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartHalf;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 6:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartFull;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 5:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartHalf;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 4:
			heart1.sprite = heartFull;
			heart2.sprite = heartFull;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 3:
			heart1.sprite = heartFull;
			heart2.sprite = heartHalf;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 2:
			heart1.sprite = heartFull;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 1:
			heart1.sprite = heartHalf;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		case 0:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		default:
			heart1.sprite = heartEmpty;
			heart2.sprite = heartEmpty;
			heart3.sprite = heartEmpty;
			heart4.sprite = heartEmpty;
			heart5.sprite = heartEmpty;
			heart6.sprite = heartEmpty;
			heart7.sprite = heartEmpty;
			return;
		}
	}

	// Handles the flashing of Gimmick's sprite while flashTimer is greater than 0.
	public void updateFlashEffect() {
		// Update flashTimer
		if (flashTimer > 0)
			flashTimer -= Time.deltaTime;

		// Linearly interpolate the alpha of Gimmick's sprite from 0.35 to 0.85 and back again, at 4 cycles per second
		if (flashTimer > 0) {
			float flash = (flashTimer * 4) - (Mathf.Floor (flashTimer * 4)) + 0.35f;
			if (flash > 0.85f)
				flash = 1.7f - flash;
			//Debug.Log ("Timer: " + flashTimer + ", Flash: " + flash);
			spriteRenderer.color = new Color (1f, 1f, 1f, flash);
		}
		else
			spriteRenderer.color = new Color (1f, 1f, 1f, 1f);

		//Debug.Log ("alpha: " + spriteRenderer.color.a);
	}

	// Returns the numerical index of the current level, for the purpose of accessing the correct row of
	//    the respawn-points 2d array.
	public int getLevelIndex() {
		string levelName = SceneManager.GetActiveScene ().name;
		if (levelName == "CaveLevel")
			return 0;
		else if (levelName == "Seaside Level")
			return 1;
		else if (levelName == "Factory Level")
			return 2;
		else if (levelName == "Forest Level")
			return 3;
		else
			return -1;
	}

}
