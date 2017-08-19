using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour {

	public int progressIndex;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other) {
		int newProgress;
		if (other.tag == "Gimmick") {
			newProgress = Mathf.Max (progressIndex, PlayerPrefs.GetInt ("Level Progress"));
			PlayerPrefs.SetInt ("Level Progress", newProgress);
		}
	}
}
