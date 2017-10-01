using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueShellController : MonoBehaviour {

	private Rigidbody2D myRigidbody;
	public float targetX;				// Target x-position, when this is exceeded the blue shell explodes.
	public float targetY;				// Target y-position, when this is exceeded the blue shell explodes.
	public Transform blueExplosion;

	// Use this for initialization
	void Start () {
		myRigidbody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		// If either the targetX x-coord or the targetY y-coord is passed over by the blue shell, it explodes
		if ((myRigidbody.velocity.x >= 0 && transform.position.x > targetX) || (myRigidbody.velocity.x < 0 && transform.position.x < targetX) || (myRigidbody.velocity.y < 0 && transform.position.y < targetY)) {
			Transform blueExplosionClone = Instantiate (blueExplosion, transform.position, transform.rotation);
			Destroy(gameObject);
		}
	}
}
