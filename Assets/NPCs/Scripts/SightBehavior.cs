using UnityEngine;
using System.Collections;

public class SightBehavior : MonoBehaviour {
		
	void OnTriggerEnter(Collider other) {
		transform.parent.GetComponent<NPCBehavior>().addNearObject(other.gameObject);
	}
	
	void OnTriggerExit(Collider other) {
		transform.parent.GetComponent<NPCBehavior>().removeNearObject(other.gameObject);
	}
}
