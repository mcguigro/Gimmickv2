using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour {

	private RectTransform trans;
	public float scaleFactor;

	// Use this for initialization
	void Start () {
		trans = GetComponent<RectTransform> ();
		int height = Screen.height;
		float scaling = ((float)Screen.height) / scaleFactor;
		trans.localScale = new Vector3 (scaling, scaling, scaling);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
