using UnityEngine;
using System.Collections;

public class ZombieBehavior : NPCBehavior {
	
	public Transform zombiePrefab;
	
	private enum MovementStates {Chasing, Wandering};
	private MovementStates movementState;
	
	// Use this for initialization
	new void Start() {
		base.Start();
		
		movementState = MovementStates.Wandering;
		transform.parent = GameObject.Find("Zombies").transform;
	}
	
	void FixedUpdate() {
		updateMovementState();
		
		switch (movementState) {
			
		case MovementStates.Chasing:
			chase();
			break;
		case MovementStates.Wandering:
			wanderAround();
			break;
		default:
			wanderAround();
			break;
		}
	}
	
	private void updateMovementState() {
		if (nearSoldiers.Count > 0 || nearCivilians.Count > 0) 
			movementState = MovementStates.Chasing;
		else
			movementState = MovementStates.Wandering;
	}
	
	private void chase() {
		GameObject nearestSoldier = findNearestObject(nearSoldiers);
		GameObject nearestCivilian = findNearestObject(nearCivilians);
		
		if (nearestSoldier != null && nearestCivilian == null) 
			pathfinder.target = nearestSoldier.transform;
		else if (nearestCivilian != null && nearestSoldier == null)
			pathfinder.target = nearestCivilian.transform;
		else if (nearestCivilian == null && nearestCivilian == null)
			pathfinder.target = groundTarget;
		else if (Vector3.Distance(nearestSoldier.transform.position, transform.position) < Vector3.Distance(nearestCivilian.transform.position, transform.position))
			pathfinder.target = nearestSoldier.transform;	
		else
			pathfinder.target = nearestCivilian.transform;
	}
	
	private void wanderAround() {
		if(pathfinder.target == null || Vector3.Distance(transform.position, pathfinder.target.position) < 5.0f) {
			groundTarget.position = generateRandomPosition(-45, 45, -45, 45);
			pathfinder.target = groundTarget;
		}
	}
	
	void OnCollisionEnter(Collision collision) {
		Collider other = collision.collider;
		if(other.gameObject.tag == "Civilian" || other.gameObject.tag == "Soldier") {
			Vector3 position = other.transform.position;
			Quaternion rotation = other.transform.rotation;
			
			if (other.gameObject.transform == pathfinder.target)
				pathfinder.target = groundTarget;
			Destroy(other.gameObject);
			Instantiate(zombiePrefab, position, rotation);
		}
	}
	
	override public void handleDestroy(GameObject destroyedObject) {
		removeNearObject(destroyedObject);
		if (pathfinder.target == destroyedObject.transform)
			pathfinder.target = groundTarget;
	}
}
