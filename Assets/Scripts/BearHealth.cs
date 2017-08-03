using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearHealth : MonoBehaviour {

	public int damageToReceive;
	private EnemyHealthManager theHealthManager;
	private StarController theStar;

	// Use this for initialization
	void Start () {
		theHealthManager = GetComponent<EnemyHealthManager> ();
		theStar = FindObjectOfType<StarController> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.tag == "Star") {
			if (theStar.canGiveDamage) {
				theHealthManager.giveDamage (damageToReceive);
			}
		}
	}
}
