using UnityEngine;
using System.Collections;

public class SpawnZombie : MonoBehaviour {
	
	public Transform zombiePrefab;
	public Transform civilianPrefab;
	public LayerMask targetMask;
	public int maxZombies;
	public int sacrificeNeeded; 
	
	private int spawnedZombies;
	private int storedZombies;
	
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
			bool objectHit = Physics.Raycast(ray, out hit, 1000, targetMask);
	        if (objectHit && hit.transform.gameObject.tag == "Civilian" && maxZombies > spawnedZombies && Time.timeScale == 0.0f) {
				Instantiate(zombiePrefab, hit.transform.position, hit.transform.rotation);
				Destroy(hit.transform.gameObject);
				spawnedZombies++;
			}
			else if (objectHit && hit.transform.gameObject.tag == "Civilian" && storedZombies >= sacrificeNeeded && Time.timeScale != 0.0f) {
				Instantiate(zombiePrefab, hit.transform.position, hit.transform.rotation);
				Destroy(hit.transform.gameObject);
				storedZombies -= sacrificeNeeded;
			}
			else if (objectHit && hit.transform.gameObject.tag == "Zombie" && Time.timeScale == 0.0f) {
				Instantiate(civilianPrefab, hit.transform.position, hit.transform.rotation);
				Destroy(hit.transform.gameObject);
				spawnedZombies--;
			}
			else if (objectHit && hit.transform.gameObject.tag == "Zombie" && storedZombies < sacrificeNeeded && Time.timeScale != 0.0f) {
				Destroy(hit.transform.gameObject);
				storedZombies++;
			}
	    }
	}
	
	public int getZombieSpawnCount() {
		return spawnedZombies;	
	}
	
	public int getSacrificedZombies() {
		return storedZombies;	
	}
}
