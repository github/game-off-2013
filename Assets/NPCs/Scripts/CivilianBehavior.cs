using UnityEngine;
using System.Collections;

public class CivilianBehavior : NPCBehavior {
	
	public int gatherLimit = 5;
	public float soldierConfidence = 5.0f;
	
	private enum MovementStates {Fleeing, Gathering, Hiding, Separating, Wandering};
	private MovementStates movementState;
	
	// Use this for initialization
	new void Start () {
		base.Start();
		
		movementState = MovementStates.Wandering;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		updateMovementState();
		
		switch (movementState) {
			
		case MovementStates.Fleeing:
			fleeZombies();
			break;
		case MovementStates.Gathering:
			gatherNearCivilans();
			break;
		case MovementStates.Hiding:
			hideNearSoldiers();
			break;
		case MovementStates.Separating:
			avoidCivilians();
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
		if (nearZombies.Count > 0) 
			movementState = MovementStates.Fleeing;
		else if (nearSoldiers.Count > 1 && nearCivilians.Count <= (nearSoldiers.Count * soldierConfidence) + gatherLimit) 
			movementState = MovementStates.Hiding;
		else if (nearCivilians.Count > 1 && nearCivilians.Count <= gatherLimit) 
			movementState = MovementStates.Gathering;
		else if (nearCivilians.Count > (nearSoldiers.Count * soldierConfidence) + gatherLimit)
			movementState = MovementStates.Separating;
		else
			movementState = MovementStates.Wandering;
	}
	
	private void fleeZombies() {
		GameObject nearestZombie = findNearestObject(nearZombies);
		
		if (nearestZombie == null)
			cleanLists();
		else {
			Vector3 distanceVector = transform.position - nearestZombie.transform.position;
			groundTarget.position = transform.position + distanceVector;
			pathfinder.target = groundTarget;	
		}
	}
	
	private void gatherNearCivilans() {
		Vector3 centroid = calculateCentroid(nearCivilians);
		float distance = Vector3.Distance(centroid, transform.position);
		if (distance > 0.0f) {
			groundTarget.position = centroid;
			pathfinder.target = groundTarget;
		}
	}
	
	private void hideNearSoldiers() {
		pathfinder.target = findFarthestObject(nearSoldiers).transform;
	}
	
	private void avoidCivilians() {
		Vector3 centroid = calculateCentroid(nearCivilians);
		Vector3 distanceVector = transform.position - centroid;
		
		groundTarget.position = transform.position + distanceVector;
		pathfinder.target = groundTarget;
	}
	
	private void wanderAround() {
		if(pathfinder.target == null || Vector3.Distance(transform.position, pathfinder.target.position) < 5.0f) {
			groundTarget.position = generateRandomPosition(-45, 45, -45, 45);
			pathfinder.target = groundTarget;
		}
	}
	
	override public void handleDestroy(GameObject destroyedObject) {
		removeNearObject(destroyedObject);
		if (pathfinder.target == destroyedObject.transform)
			pathfinder.target = groundTarget;
	}
}
