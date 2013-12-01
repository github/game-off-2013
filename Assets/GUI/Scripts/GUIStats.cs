using UnityEngine;
using System.Collections;

public class GUIStats : MonoBehaviour {

	void OnGUI () {
		if (Time.timeScale != 0) {
			if (GUI.Button(new Rect(10, 56, 55, 20), "Restart")) {
				Application.LoadLevel(1);	
			}
			
			GUI.Label(new Rect(10, 0, 500, 20), "Elapsed Time (seconds): " + Time.timeSinceLevelLoad);
			GUI.Label(new Rect(10, 12, 500, 20), "Civilians: " + GameObject.FindGameObjectsWithTag("Civilian").Length);
			GUI.Label(new Rect(10, 24, 500, 20), "Zombies: " + GameObject.FindGameObjectsWithTag("Zombie").Length);
			GUI.Label(new Rect(10, 36, 500, 20), "Soldiers:  " + GameObject.FindGameObjectsWithTag("Soldier").Length);
		}
	}
}