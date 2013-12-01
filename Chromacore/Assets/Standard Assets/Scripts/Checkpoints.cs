using UnityEngine;
using System.Collections;

public class Checkpoints : MonoBehaviour {
	// Get the current spawn point
	public GameObject spawnPoint;
	
	// Teli's velocity
	float velocity = 4.75196f;
	
	// Teli's starting position
	float startPOS = -62.20014f;
		
	// This checkpoint's timestamp
	float checkpoint_timestamp;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// Detect collisions with checkpoints via Character Controller
	// When checkpoint is found, set spawn point to checkpoint's position
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Checkpoint"){
			// Add 5 to the checkpoint's y position to ensure Teli isn't spawned below level
			col.transform.position = new Vector3(col.transform.position.x, col.transform.position.y + 1, col.transform.position.z);
			spawnPoint.transform.position = col.transform.position;
			checkpoint_timestamp = calcTimestamp();
			BroadcastMessage("getCheckpoint", checkpoint_timestamp);
		}
	}
	
	float calcTimestamp(){
		// Formula: ( timestamp = (xPOS - startPOS) / velocity )
		checkpoint_timestamp = (collider.transform.position.x - startPOS) / velocity;
		return checkpoint_timestamp;
		
		// (Opposite of calcXPOS formula: xPOS = (velocity * timestamp) + startPOS )
	}
}
