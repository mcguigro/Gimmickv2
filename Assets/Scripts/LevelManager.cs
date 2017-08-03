using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public Sprite heartFull;
    public Sprite heartHalf;
    public Sprite heartEmpty;

    public int maxHealth;//max health the player has
    public int healthCount;//health count how much health Gimmick currently has
    public bool Invincible;// has he just been hit?
	public float flashTimer;// time for which Gimmick will be flashing, or <= 0 if not flashing
	private SpriteRenderer spriteRenderer;// Gimmick's sprite renderer
    private bool respawning;// is he currently respawning?
    public GameObject deathsplosion;// particle effect when Gimmick dies
    public GameObject gameOverScreen;//ability to activeate the game over screen 
    public AudioSource coinSound;
    public AudioSource heartSound;
    public AudioSource levelMusic;
    public AudioSource GameOverMusic;

	private HighScore theHighScore;


    // Use this for initialization
    void Start()
    {
        Gimmick = FindObjectOfType<GimmickController>();
        healthCount = maxHealth;
		flashTimer = 0f;
		spriteRenderer = Gimmick.gameObject.GetComponent<SpriteRenderer> ();

        PointText.text = "Current Score: " + currentScore;

		theHighScore = FindObjectOfType<HighScore> ();
    }

    // Update is called once per frame
    void Update()
    {
        /* This is if we decide to have checkpoints and or a lives system
        if (healthCount<=0 && !respawning)
        {
            Respawn();
            respawning = true;
        }
        */

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
        Gimmick.gameObject.SetActive(false);
        Instantiate(deathsplosion, Gimmick.transform.position, Gimmick.transform.rotation);
        yield return new WaitForSeconds(waitToRespawn);
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
		theHighScore.SaveScore ();   // save the score of the player

        levelMusic.Stop();
        GameOverMusic.Play();
    }
    //If Gimmick runs into an enemy or danger and takes damage
    public void HurtPlayer(int damageToTake)
    {
        //If Gimmick isn't Invincible decrease health 
        if (!Invincible)
        {
            healthCount -= damageToTake;
            UpdateHeartMeter();
            Gimmick.hurtSound.Play();

            if (healthCount <= 0)
				Respawn ();
			Invincible = true;
			StartCoroutine ("HurtPlayerCo");
        }
    }

	//PRECONDITION: flashTimer must be set with the time to be invincible just before calling this.
	private IEnumerator HurtPlayerCo(){
		// make player immune to damage for a duration
		yield return new WaitForSeconds (flashTimer);
		Invincible = false;
	}
		
    //This is for Items that give Gimmick health
    public void GiveHealth(int healthToGive)
    {
       
        healthCount += healthToGive;
        heartSound.Play();
        if (healthCount > maxHealth)
        {
            healthCount = maxHealth;
        }
        UpdateHeartMeter();

    }
    //This updates the heart images that represent health on the players screen
    public void UpdateHeartMeter()
    {
        switch (healthCount)
        {
            case 6:
                heart1.sprite = heartFull;
                heart2.sprite = heartFull;
                heart3.sprite = heartFull;
                return;
            case 5:
                heart1.sprite = heartFull;
                heart2.sprite = heartFull;
                heart3.sprite = heartHalf;
                return;
            case 4:
                heart1.sprite = heartFull;
                heart2.sprite = heartFull;
                heart3.sprite = heartEmpty;
                return;
            case 3:
                heart1.sprite = heartFull;
                heart2.sprite = heartHalf;
                heart3.sprite = heartEmpty;
                return;
            case 2:
                heart1.sprite = heartFull;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                return;
            case 1:
                heart1.sprite = heartHalf;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                return;
            case 0:
                heart1.sprite = heartEmpty;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                return;
            default:
                heart1.sprite = heartEmpty;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                return;
        }

    }

	public void updateFlashEffect() {
		if (flashTimer > 0)
			flashTimer -= Time.deltaTime;
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

}
