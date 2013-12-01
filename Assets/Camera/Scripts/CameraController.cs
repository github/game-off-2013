using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed;
	
	private float lastUpdate;
	
	void Start() {
		lastUpdate = Time.realtimeSinceStartup;	
	}
	
	void LateUpdate() {
		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		float moveVertical = Input.GetAxisRaw("Vertical");
		
		Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0);
		if (Time.timeScale == 0.0f)
			transform.Translate(movement * speed * (Time.realtimeSinceStartup - lastUpdate));
		else 
			transform.Translate(movement * speed * Time.deltaTime);
		
		if (transform.position.z > 50) transform.position = new Vector3(transform.position.x, transform.position.y, 50);
		else if (transform.position.z < -50) transform.position = new Vector3(transform.position.x, transform.position.y, -50);
		
		if (transform.position.x > 50) transform.position = new Vector3(50, transform.position.y, transform.position.z);
		else if (transform.position.x < -50) transform.position = new Vector3(-50, transform.position.y, transform.position.z);
		
		lastUpdate = Time.realtimeSinceStartup;
	}
}
