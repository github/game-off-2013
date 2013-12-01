using UnityEngine;
using System.Collections;

public class tk2dTileMapDemoFollowCam : MonoBehaviour {

	tk2dCamera cam;
	public Transform target;
	public float followSpeed = 1.0f;

	public float minZoomSpeed = 20.0f;
	public float maxZoomSpeed = 40.0f;

	public float maxZoomFactor = 0.6f;

	void Awake() {
		cam = GetComponent<tk2dCamera>();
	}

	void FixedUpdate() {
		Vector3 start = transform.position;
		Vector3 end = Vector3.MoveTowards(start, target.position, followSpeed * Time.deltaTime);
		end.z = start.z;
		transform.position = end;

		if (target.rigidbody != null && cam != null) {
			float spd = target.rigidbody.velocity.magnitude;
			float scl = Mathf.Clamp01((spd - minZoomSpeed) / (maxZoomSpeed - minZoomSpeed));
			float targetZoomFactor = Mathf.Lerp(1, maxZoomFactor, scl);
			cam.ZoomFactor = Mathf.MoveTowards(cam.ZoomFactor, targetZoomFactor, 0.2f * Time.deltaTime);
		}
	}
}
