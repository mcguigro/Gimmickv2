using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeStarDamage : MonoBehaviour {

	public int damageToTake;
	private LevelManager theLevelManager;
	private EnemyHealthManager myEnemyHealthManager;

	// Use this for initialization
	void Start () {
		theLevelManager = GetComponent<LevelManager> ();
		myEnemyHealthManager = GetComponent<EnemyHealthManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other){
		Debug.Log ("Something enter trigger zone");
		if (other.tag == "Star" || other.tag == "StarGround") {
			Debug.Log ("Star enter trigger zone");
			myEnemyHealthManager.giveDamage (damageToTake);
		}
	}

//	void OnCollisionEnter2D(Collision2D other){
//		if (other.gameObject.tag == "Star") {
//			myEnemyHealthManager.giveDamage (damageToTake);
//		}
//	}
}
