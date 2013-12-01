using UnityEngine;
using System.Collections;

public class ZombiePlacement : MonoBehaviour {
	
	public SpawnZombie spawnZombie;
	
	void OnGUI() {
		if(Time.timeScale == 0) {
			
			GUI.Label(new Rect(10,  0, 100, 20), "Zombies Placed: ");
			GUI.Label(new Rect(110, 0, 100, 20), spawnZombie.getZombieSpawnCount() + "/" + spawnZombie.maxZombies);
			if (GUI.Button(new Rect(10, 22, 50, 20), "Start")) {
				Time.timeScale = 1.0f;
			}
		}
	}
}
