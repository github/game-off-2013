using UnityEngine;
using System.Collections;

public class SpawnZombie : MonoBehaviour {
	
	public Transform zombiePrefab;
	public LayerMask targetMask;
	public int maxZombies;
	
	private int spawnedZombies;
	
	void Awake() {
		Time.timeScale = 0.0f;	
	}
	
	void Start() {
		spawnedZombies = 0;
	}
	
	// Update is called once per frame
	void Update() {
	    if (Input.GetMouseButtonUp(0)) {
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			bool objectHit = Physics.Raycast(ray, out hit, 100, targetMask);
	        if (objectHit && hit.transform.gameObject.tag == "Ground" && spawnedZombies < maxZombies && Time.timeScale == 0.0f) {
	            Instantiate(zombiePrefab, new Vector3(hit.point.x, 0.5f, hit.point.z), transform.rotation);
				spawnedZombies++;
			}
			else if (objectHit && hit.transform.gameObject.tag == "Zombie" && Time.timeScale == 0.0f) {
				Destroy(hit.transform.gameObject);
				spawnedZombies--;
			}
	    }
	}
	
	public int getZombieSpawnCount() {
		return spawnedZombies;	
	}
}
