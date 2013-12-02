using UnityEngine;
using System.Collections;

public class GUIStats : MonoBehaviour {
	
	public SpawnZombie spawnZombie;
	
	void OnGUI () {
		if (Time.timeScale != 0) {
			if (GUI.Button(new Rect(10, 68, 55, 20), "Restart")) {
				Application.LoadLevel(Application.loadedLevel);
			}
			else if (GUI.Button(new Rect(70, 68, 85, 20), "Select Level")) {
				Application.LoadLevel(0);	
			}
			
			GUI.Label(new Rect(10, 0, 500, 20), "Elapsed Time (seconds): " + Time.timeSinceLevelLoad);
			GUI.Label(new Rect(10, 12, 500, 20), "Civilians: " + GameObject.FindGameObjectsWithTag("Civilian").Length);
			GUI.Label(new Rect(10, 24, 500, 20), "Zombies: " + GameObject.FindGameObjectsWithTag("Zombie").Length);
			GUI.Label(new Rect(10, 48, 500, 20), "Soldiers:  " + GameObject.FindGameObjectsWithTag("Soldier").Length);
			if (spawnZombie.getSacrificedZombies() < spawnZombie.sacrificeNeeded)
				GUI.Label(new Rect(10, 36, 500, 20), "Sacrfices needed: " + (spawnZombie.sacrificeNeeded - spawnZombie.getSacrificedZombies()));
			else
				GUI.Label(new Rect(10, 36, 500, 20), "Infection Ready!");
		}
	}
}