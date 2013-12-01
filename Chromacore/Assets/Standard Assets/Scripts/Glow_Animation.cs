using UnityEngine;
using System.Collections;

public class Glow_Animation : MonoBehaviour {
	// Used to detect collisions with Notes via Character Controller and
	// send message to animation script to play Glow Animation.
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Note"){
			BroadcastMessage("GlowAnimation");
		}
	}
}