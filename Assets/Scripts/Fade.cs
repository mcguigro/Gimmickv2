using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Fade : MonoBehaviour {

    public bool start = false;
    public float fadeDamp = 0.0f;
    public string NextLevelScreen;
    public float alpha = 0.0f;
    public Color ColorToFade;
    public bool isFadingIn = false;


	
	void OnEnable () {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}
	
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
	

	void OnGUI() {
        if(!start)
        {
            return;
        }
        //assign the color with variable alpha
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        //create a temp texture
        Texture2D newTex;
        newTex = new Texture2D(1, 1);
        newTex.SetPixel(0, 0, ColorToFade);
        newTex.Apply();
        //print texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), newTex);

        if (isFadingIn)
        {
            alpha = Mathf.Lerp(alpha, -0.1f, fadeDamp * Time.deltaTime);
         }
        else
        {
            alpha = Mathf.Lerp(alpha, 1.1f, fadeDamp * Time.deltaTime);
        
        }
        if(alpha >=1 && !isFadingIn)
        {
            SceneManager.LoadScene(NextLevelScreen);
            DontDestroyOnLoad(gameObject);
        }
        else if (alpha<=0 && isFadingIn)
        {
            Destroy(gameObject);
        }
        

    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //We can now fade in
        isFadingIn = true;
    }

}
