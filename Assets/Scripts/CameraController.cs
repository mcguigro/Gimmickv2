using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public const float xTolerance = 2f;		// The maximum number of units Gimmick can deviate left/right from the center of the screen
	public const float yTolerance = 1.5f;	// The maximum number of units Gimmick can deviate up/down from the center of the screen

	public GameObject gimmick;				// Gimmick, the main character


	// Use this for initialization
	void Start () {
		
	}

	
	// Update is called once per frame
	void Update () {
		// Move the camera a minimum distance from its previous location so that Gimmick is within xTolerance by yTolerance of the center of the screen
		float newX = max (min (transform.position.x, gimmick.transform.position.x + xTolerance), gimmick.transform.position.x - xTolerance);
		float newY = max (min (transform.position.y, gimmick.transform.position.y + yTolerance), gimmick.transform.position.y - yTolerance);
		transform.position = new Vector3 (newX, newY, -10f);
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

}
