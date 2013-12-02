using UnityEngine;
using System.Collections;

public class ZombiePlacement : MonoBehaviour {
	
	public SpawnZombie spawnZombie;
	
	void OnGUI() {
		if(Time.timeScale == 0) {
			
			GUI.Label(new Rect(10,  0, 300, 22), "Infections Remaining: " + (spawnZombie.maxZombies - spawnZombie.getZombieSpawnCount()));
			if (GUI.Button(new Rect(10, 22, 50, 20), "Start")) {
				Time.timeScale = 1.0f;
			}
			else if (GUI.Button(new Rect(65, 22, 85, 20), "Select Level")) {
				Application.LoadLevel(0);	
			}
		}
	}
}
