using UnityEngine;
using System.Collections;

public class ObstacleCollision : MonoBehaviour {
	private tk2dSpriteAnimator anim;
	
	// Get the Teli animation object
	public tk2dSpriteAnimator Teli;
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<tk2dSpriteAnimator>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	// Upon picking up this object, trigger events
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Player")
		{
			Debug.Log("Obstacle Hit");
			// And they are punching
			
			if(Teli.IsPlaying("Punch"))
			{
				// Break the obstacle
				BreakObstacle();
				// Play the breaking sound
				audio.Play();
			}
		}
	}
	
	// Play break animation, then destroy gameObject
	void BreakObstacle()
	{
		// Play the break animation
		anim.Play("Obstacle"); 
		Debug.Log("Obstacle break!");
		// Destroy Obstacle gameObject after 5 seconds
		Invoke("Destroy", 5);
	}
	
	// Destroy the obstacle
	void Destroy()
	{
		Destroy(gameObject);
	}
	

}
