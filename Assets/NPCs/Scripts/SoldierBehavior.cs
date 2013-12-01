using UnityEngine;
using System.Collections;

public class SoldierBehavior : NPCBehavior {
	
	public float bravery = 5.0f;
	public float accuracy = 1.0f;
	public float shootingSpeed = 1.0f;
	
	private enum MovementStates {Retreating, Following, Wandering};
	private MovementStates movementState;
	private GameObject target;
	private float lastShot;
	
	// Use this for initialization
	new void Start () {
		base.Start();
		
		lastShot = Time.time;
		movementState = MovementStates.Wandering;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		updateMovementState();
		
		switch (movementState) {
			
		case MovementStates.Retreating:
			retreat();
			break;
		case MovementStates.Following:
			followZombie();
			break;
		case MovementStates.Wandering:
			wanderAround();
			break;
		default:
			wanderAround();
			break;
		}
		
		shootAtTarget();
	}
	
	private void updateMovementState() {
		if (nearZombies.Count > 0) {
			GameObject nearestZombie = findNearestObject(nearZombies);
			if (nearestZombie == null)
				cleanLists();
			else if (Vector3.Distance(nearestZombie.transform.position, transform.position) > bravery) 
				movementState = MovementStates.Following;
			else 
				movementState = MovementStates.Retreating;
		}
		else
			movementState = MovementStates.Wandering;
	}
	
	private void retreat() {
		GameObject nearestZombie = findNearestObject(nearZombies);
		Vector3 distanceVector = transform.position - nearestZombie.transform.position;
		
		groundTarget.position = transform.position + distanceVector;
		pathfinder.target = groundTarget;	
		target = nearestZombie;
	}
	
	private void followZombie() {
		target = findNearestObject(nearZombies);
		
		if (target != null) 
			pathfinder.target = target.transform;
		else
			pathfinder.target = groundTarget;
	}
	
	private void wanderAround() {
		if(pathfinder.target == null || Vector3.Distance(transform.position, pathfinder.target.position) < 5.0f) {
			groundTarget.position = generateRandomPosition(-45, 45, -45, 45);
			pathfinder.target = groundTarget;
		}
	}
	
	private void shootAtTarget() {
		if (target != null && Time.time - lastShot > 1.0f / shootingSpeed) {
			lastShot = Time.time;
			float angle = calculateShotVariance();
			Destroy(target);
		}
	}
	
	private float calculateShotVariance() {
			return 0.0f;
	}
	
	override public void handleDestroy(GameObject destroyedObject) {
		removeNearObject(destroyedObject);
		if (pathfinder.target == destroyedObject.transform)
			pathfinder.target = groundTarget;
		if (target == destroyedObject)
			target = null;
	}
}
