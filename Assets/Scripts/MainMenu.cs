using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/* place first level into first level and the select level scene into levelSelect */
public class MainMenu : MonoBehaviour
{

    public string firstLevel;
    public string levelSelect;
	public string HSLevelSelect;
    public string[] levelNames;
    public Color loadToColor = Color.white;
    public int fadeSpeed;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    // This loads the first level and locks all but the first level for the level select menu
    // In this instence we are unlocking all levels
    public void NewGame()
    {
        //SceneManager.LoadScene(firstLevel);// loads the first level
        ScreenTransition.FadeScreen(firstLevel, loadToColor, fadeSpeed);
        Time.timeScale = 1f;
        // For regular gaming set the int to 0 to lock all but the first level
        for (int i = 0; i < levelNames.Length; i++)
        {
            PlayerPrefs.SetInt(levelNames[i], 1);
        }
    }
    //This loads the Level Select menu
    public void Continue()
    {
        //SceneManager.LoadScene(levelSelect);
        ScreenTransition.FadeScreen(levelSelect, loadToColor, fadeSpeed);
        Time.timeScale = 1f;
    }
    //This Quits the application
    public void QuitGame()
    {
        Application.Quit();

    }

	public void HighScores(){
        SceneManager.LoadScene (HSLevelSelect);
        //ScreenTransition.FadeScreen(HSLevelSelect, loadToColor, fadeSpeed);
    }
}
