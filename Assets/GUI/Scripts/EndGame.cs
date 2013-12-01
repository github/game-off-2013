using UnityEngine;
using System.Collections;

public class EndGame : MonoBehaviour {
	
	void Awake() {
		//GameObject.FindGameObjectWithTag("Defeat").SetActive(false);
		//GameObject.FindGameObjectWithTag("Victory").SetActive(false);
	}
	
	void OnGUI() {
		if (Time.timeScale != 0) {
			if (GameObject.FindGameObjectsWithTag("Zombie").Length == 0 && Time.timeSinceLevelLoad > 1.0f) {
				//GameObject.FindGameObjectWithTag("Defeat").SetActive(true);
			}
			else if (GameObject.FindGameObjectsWithTag("Civilian").Length == 0) {
				//GameObject.FindGameObjectWithTag("Victory").SetActive(true);
			}
		}
	}
}
