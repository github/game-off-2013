using UnityEngine;
using System.Collections;

abstract public class NPCBehavior : MonoBehaviour {

	protected ArrayList nearZombies;
	protected ArrayList nearCivilians;
	protected ArrayList nearSoldiers;
	
	protected AIPath pathfinder;
	protected Transform groundTarget; 
	
	// Use this for initialization
	protected void Start() {
		nearZombies = new ArrayList();
		nearCivilians = new ArrayList();
		nearSoldiers = new ArrayList();
		
		GameObject targets = GameObject.Find("Ground Targets");
		groundTarget = new GameObject("Ground Target").transform;
		groundTarget.position = generateRandomPosition(-45, 45, -45, 45);
		groundTarget.transform.parent = targets.transform;
		
		pathfinder = transform.GetComponent<AIPath>();
		pathfinder.target = groundTarget;
	}
	
	abstract public void handleDestroy(GameObject destroyedObject);
	
	protected void OnDestroy() {
		if (groundTarget != null && groundTarget.gameObject != null) Destroy(groundTarget.gameObject);
		
		foreach (GameObject zombie in nearZombies) {
			if (zombie != null) zombie.transform.GetComponent<NPCBehavior>().handleDestroy(gameObject);
		}
		
		foreach (GameObject civilian in nearCivilians) {
			if (civilian != null) civilian.transform.GetComponent<NPCBehavior>().handleDestroy(gameObject);
		}
		
		foreach (GameObject soldier in nearSoldiers) {
			if (soldier != null) soldier.transform.GetComponent<NPCBehavior>().handleDestroy(gameObject);
		}
	}
	
	protected GameObject findNearestObject(ArrayList gameObjects) {
		GameObject nearestObject = null;
		float nearestDistance = Mathf.Infinity;
		
		foreach(GameObject gameObject in gameObjects) {
			if (gameObject != null) {
				float distance = Vector3.Distance(transform.position, gameObject.transform.position);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestObject = gameObject;
				}
			}
		}
		
		return nearestObject;
	}
	
	protected GameObject findFarthestObject(ArrayList gameObjects) {
		GameObject farthestObject = null;
		float farthestDistance = 0.0f;
		
		foreach(GameObject gameObject in gameObjects) {
			float distance = Vector3.Distance(transform.position, gameObject.transform.position);
			if (distance > farthestDistance) {
				farthestDistance = distance;
				farthestObject = gameObject;
			}
		}
		
		return farthestObject;
	}
	
	protected Vector3 calculateCentroid(ArrayList gameObjects) {
		Vector3 centroid = new Vector3();
		int count = 0;
		
		foreach(GameObject gameObject in gameObjects) {
			if (gameObject != null) {
				centroid += gameObject.transform.position;
				count++;	
			}
		}
		
		centroid /= count;
		
		return centroid;
	}
	
	protected Vector3 generateRandomPosition(float xMin, float xMax, float zMin, float zMax) {
		return new Vector3(Random.Range(xMin, xMax), 0.0f, Random.Range(zMin, zMax));
	}
	
	public void addNearObject(GameObject nearObject) {
		if (nearObject != gameObject) {
			switch (nearObject.tag) {
			
			case "Player":	
				break;
			case "Zombie":
				addNearZombie(nearObject);
				break;
			case "Civilian":
				addNearCivilian(nearObject);
				break;
			case "Soldier":
				addNearSoldier(nearObject);
				break;
			default:
				break;
			}
		}
	}
	
	public void addNearZombie(GameObject zombie) {
		if (zombie != gameObject) nearZombies.Add(zombie);
	}
	
	public void addNearCivilian(GameObject civilian) {
		if (civilian != gameObject) nearCivilians.Add(civilian);	
	}
	
	public void addNearSoldier(GameObject soldier) {
		if (soldier != gameObject) nearSoldiers.Add(soldier);
	}
	
	public void removeNearObject(GameObject nearObject) {
		switch (nearObject.tag) {
		
		case "Player":	
			break;
		case "Zombie":
			removeNearZombie(nearObject);
			break;
		case "Civilian":
			removeNearCivilian(nearObject);
			break;
		case "Soldier":
			removeNearSoldier(nearObject);
			break;
		default:
			break;
		}
	}
	
	public void removeNearZombie(GameObject zombie) {
		nearZombies.Remove(zombie);
	}
	
	public void removeNearCivilian(GameObject civilian) {
		nearCivilians.Remove(civilian);
	}
	
	public void removeNearSoldier(GameObject soldier) {
		nearSoldiers.Remove(soldier);
	}
	
	public void cleanLists() {
		foreach (GameObject zombie in nearZombies)
			if (zombie == null) nearZombies.Remove(zombie);
		foreach (GameObject civilian in nearCivilians)
			if (civilian == null) nearCivilians.Remove(civilian);
		foreach (GameObject soldier in nearSoldiers)
			if (soldier == null) nearSoldiers.Remove(soldier);
	}
}
