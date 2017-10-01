using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used for moving a background image comprised of rectangular 2D tiles that repeat themselves.
// When the background moves far enough, it is jumped by a distance equal to the length/height of one tile,
//   giving the illusion that the background repeats itself forever.

public class BackgroundScroller : MonoBehaviour {

	public float scrollRateX;		// Rate (use 0 through 1) at which the background moves horizontally relative to the level.  0 = stuck to level, 1 = stuck to camera.
	public float scrollRateY;		// Rate (use 0 through 1) at which the background moves vertically relative to the level.  0 = stuck to level, 1 = stuck to camera.
	private float xUnitsPerLoop;	// Width (units) of one tile of the background.  Set only once, in Start().
	private float yUnitsPerLoop;	// Height (units) of one tile of the background.  Set only once, in Start().
	private float xOffset;			// Total horizontal displacement (units) applied to the background as a result of jumps used to simulate looping.
	private float yOffset;			// Total vertical displacement (units) applied to the background as a result of jumps used to simulate looping.

	public float leftLoopPoint;		// x-coordinate on the background image that, when appearing in the center of the screen, causes the background image to snap left
									// so that rightLoopPoint is in the center of the screen.
	// (These three play an analogous role to leftLoopPoint)
	public float rightLoopPoint;	
	public float bottomLoopPoint;
	public float topLoopPoint;

	// Use this for initialization
	void Start () {
		// Pre-calculate units per loop (used frequently)
		xUnitsPerLoop = rightLoopPoint - leftLoopPoint;
		yUnitsPerLoop = topLoopPoint - bottomLoopPoint;
	}
	
	// Update is called once per frame
	void Update () {
		// Update x-pos of background, as a fraction of the camera's x-pos but offseting by xOffset due to jumps
		float newXPos = xOffset + scrollRateX * Camera.main.transform.position.x;

		// If background is too far to the right, jump it to the left
		while (Camera.main.transform.position.x - newXPos < leftLoopPoint) {
			xOffset -= xUnitsPerLoop;
			newXPos -= xUnitsPerLoop;
		}
		// If background is too far to the left, jump it to the right
		while (Camera.main.transform.position.x - newXPos > rightLoopPoint) {
			xOffset += xUnitsPerLoop;
			newXPos += xUnitsPerLoop;
		}

		// Update y-pos of background, as a fraction of the camera's y-pos but offseting by yOffset due to jumps
		float newYPos = yOffset + scrollRateY * Camera.main.transform.position.y;

		// If background is too high, jump it down
		while (Camera.main.transform.position.y - newYPos < bottomLoopPoint) {
			yOffset -= yUnitsPerLoop;
			newYPos -= yUnitsPerLoop;
		}
		// If background is too low, jump it up
		while (Camera.main.transform.position.y - newYPos > topLoopPoint) {
			yOffset += yUnitsPerLoop;
			newYPos += yUnitsPerLoop;
		}

		transform.position = new Vector3 (newXPos, newYPos, 0f);
	}
}
