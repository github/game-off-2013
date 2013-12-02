using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed;
	public float zoomSpeed;
	
	private float lastUpdate;
	
	void Start() {
		lastUpdate = Time.realtimeSinceStartup;	
	}
	
	void LateUpdate() {
		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		float moveVertical = Input.GetAxisRaw("Vertical");
		float mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
		
		Vector3 movement = new Vector3(moveHorizontal, moveVertical, mouseScroll * (zoomSpeed / speed));
		transform.Translate(movement * speed * (Time.realtimeSinceStartup - lastUpdate));
		
		if (transform.position.z > 50) transform.position = new Vector3(transform.position.x, transform.position.y, 50);
		else if (transform.position.z < -50) transform.position = new Vector3(transform.position.x, transform.position.y, -50);
		
		if (transform.position.x > 50) transform.position = new Vector3(50, transform.position.y, transform.position.z);
		else if (transform.position.x < -50) transform.position = new Vector3(-50, transform.position.y, transform.position.z);
		
		if (transform.position.y > 100) transform.position = new Vector3(transform.position.x, 100, transform.position.z);
		else if (transform.position.y < 25) transform.position = new Vector3(transform.position.x, 25, transform.position.z);
		
		lastUpdate = Time.realtimeSinceStartup;
	}
}
