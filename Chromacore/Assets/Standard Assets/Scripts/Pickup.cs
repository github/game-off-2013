using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	// Corresponding sound to the musical track attached to 
	// the object
	private AudioClip collectSound;
	
	// Get the parent of this collectible
	GameObject parent;
	
	// Use this for initialization
	void Start () {
		parent = transform.parent.gameObject;
	}
	
	// Upon picking up this object, trigger events
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Player")
		{
			// Change the color of the textures on pickup
			parent.SendMessage("ChangeColor");
			// A note has been collected so increment the score
			col.SendMessage("CollectNote");
			// Make this object invisible
			gameObject.renderer.enabled = false;
			// Play the corresponding sound
			audio.Play();
		}
	}
		
}
