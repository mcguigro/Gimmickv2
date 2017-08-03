using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour {

	public const float MAXFALLSPEED = 25f;

	private Rigidbody2D myRigidbody;
    public int damangeToGive;
    private int damageToGive;

	public bool canGiveDamage;  // ++++


    // Use this for initialization
    void Start () {
		myRigidbody = GetComponent<Rigidbody2D> ();
		canGiveDamage = true; // ++++
	}


	// Update is called once per frame
	void Update () {
		float yVel = getYVelocity ();
		myRigidbody.velocity = new Vector3 (myRigidbody.velocity.x, yVel, 0f);

		// +++++++++++++++++++++++++++++++++
		if (myRigidbody.velocity.x == 0f) {
			canGiveDamage = false;
		} else {
			canGiveDamage = true;
		}
		// +++++++++++++++++++++++++++++++++
	}


	public float getYVelocity() {
		float yVel = myRigidbody.velocity.y;
		return Mathf.Max (yVel, -MAXFALLSPEED);
	}

	void OnBecameInvisible()
	{
		gameObject.SetActive (false);
	}

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.tag =="Enemy")
    //    {

    //        other.GetComponent<EnemyHealthManager>().giveDamage(this.damageToGive);
    //    }
       
    //    Destroy(gameObject);
    //}

}
