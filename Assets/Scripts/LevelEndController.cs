using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndController : MonoBehaviour {

	private bool isWaving;
	public const float TIMEUNTILFREEZE = 1f;
	public const float FREEZETIME = 1f;
	public string nextLevel;
    public int fadeSpeed;
	public Animator myAnim;
	private HighScore theHighScore;
    public Color loadUsingColor = Color.white;

	// Use this for initialization
	void Start () {
		isWaving = false;
		myAnim = GetComponent<Animator> ();

		theHighScore = FindObjectOfType<HighScore> ();
	}
	
	// Update is called once per frame
	void Update () {
		myAnim.SetBool ("Waving", isWaving);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Gimmick" && !isWaving) {
			isWaving = true;
			StartCoroutine ("LevelEndCo");
		}
	}

	public IEnumerator LevelEndCo () {
		theHighScore.SaveScore ();			// save the score the player had at the end of the level

		yield return new WaitForSeconds (TIMEUNTILFREEZE);
		//Time.timeScale = 0f;
		//yield return new WaitForSeconds (FREEZETIME); //CAN'T WAIT FOR TIME WHEN TIME'S FROZEN!
		theHighScore.LogScores();			// log all of the scores for that level
		theHighScore.LogTopFiveScores ();   // log the top five scores for that level

        //fade out of the game and load the Level complete screen which will load the next screen via the LevelTitleScreenScript
         ScreenTransition.FadeScreen(nextLevel, loadUsingColor, fadeSpeed);
         
      //this will be done through the levelTitle Screen Script
       // SceneManager.LoadScene(nextLevel);
		Time.timeScale = 1f;
	}
}
