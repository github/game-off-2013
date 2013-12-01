using UnityEngine;
using System.Collections;

public class tk2dTileMapDemoPlayer : MonoBehaviour {

	public tk2dTextMesh textMesh;
	public tk2dTextMesh textMeshLabel;
	Vector3 textMeshOffset;
	bool textInitialized = false;

	public float addForceLimit = 1.0f;
	public float amount = 500.0f;
	public float torque = 50;
	tk2dSprite sprite;
	int score = 0;
	float forceWait = 0;
	float moveX = 0.0f;
	bool AllowAddForce { get { return forceWait < 0.0f; } }

	void Awake() {
		sprite = GetComponent<tk2dSprite>();

		if (textMesh == null || textMesh.transform.parent != transform) {
			Debug.LogError("Text mesh must be assigned and parented to player.");
			enabled = false;
		}

		textMeshOffset = textMesh.transform.position - transform.position;
		textMesh.transform.parent = null;

		textMeshLabel.text = "instructions";
		textMeshLabel.Commit();

		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.OSXDashboardPlayer) {
			textMesh.text = "LEFT ARROW / RIGHT ARROW";
		}
		else {
			textMesh.text = "TAP LEFT / RIGHT SIDE OF SCREEN";
		}
		textMesh.Commit();

		Application.targetFrameRate = 60;
	}
	
	void Update() {
		forceWait -= Time.deltaTime;

		string spriteName = AllowAddForce ? "player" : "player_disabled";
		if (sprite.CurrentSprite.name != spriteName) {
			sprite.SetSprite(spriteName);
		}

		if (AllowAddForce) {
			float x = 0;

			if (Input.GetKeyDown(KeyCode.RightArrow)) x = 1;
			else if (Input.GetKeyDown(KeyCode.LeftArrow)) x = -1;

			for (int i = 0; i < Input.touchCount; ++i) {
				if (Input.touches[i].phase == TouchPhase.Began) {
					x = Mathf.Sign(Input.touches[i].position.x - Screen.width * 0.5f);
					break;
				}
			}

			if (x != 0) {
				// make sure text meshes are changed on first button press / touch
				if (!textInitialized) {
					textMeshLabel.text = "score";
					textMeshLabel.Commit();
					textMesh.text = "0";
					textMesh.Commit();
					textInitialized = true;
				}

				// The actual applying of force is deferred to the next FixedUpdate for predictable
				// physics behaviour
				moveX = x;
			}
		}

		textMesh.transform.position = transform.position + textMeshOffset;
	}

	void FixedUpdate () {
		if (AllowAddForce && moveX != 0) {
			forceWait = addForceLimit;
			rigidbody.AddForce(new Vector3(moveX * amount, amount, 0) * Time.deltaTime, ForceMode.Impulse);
			rigidbody.AddTorque(new Vector3(0,0,-moveX * torque) * Time.deltaTime, ForceMode.Impulse);
			moveX = 0;
		}
	}

	void OnTriggerEnter(Collider other) {
		Destroy( other.gameObject );

		score++;
		
		textMesh.text = score.ToString();
		textMesh.Commit();
	}
}
