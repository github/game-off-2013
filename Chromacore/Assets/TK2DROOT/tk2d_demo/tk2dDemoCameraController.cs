using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Demo/tk2dDemoCameraController")]
public class tk2dDemoCameraController : MonoBehaviour {

	public Transform listItems;
	public Transform endOfListItems;
	Vector3 listTopPos = Vector3.zero;
	Vector3 listBottomPos = Vector3.zero;
	bool listAtTop = true;
	bool transitioning = false;

	public Transform[] rotatingObjects = new Transform[0];

	// Use this for initialization
	void Start () {
		listTopPos = listItems.localPosition;
		listBottomPos = listTopPos - endOfListItems.localPosition;
	}

	IEnumerator MoveListTo(Vector3 from, Vector3 to) {
		transitioning = true;
		float time = 0.5f;
		for (float t = 0.0f; t < time; t += Time.deltaTime) {
			float nt = Mathf.Clamp01(t / time);
			nt = Mathf.SmoothStep(0, 1, nt);
			listItems.localPosition = Vector3.Lerp(from, to, nt);
			yield return 0;
		}
		listItems.localPosition = to;

		transitioning = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && !transitioning) {
			// Only process mouse hits if we didn't hit anything else (eg. buttons)
			if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition))) {
				if (listAtTop) {
					StartCoroutine( MoveListTo( listTopPos, listBottomPos ) );
				}
				else {
					StartCoroutine( MoveListTo( listBottomPos, listTopPos ) );
				}
				listAtTop = !listAtTop;
			}
		}

		foreach (Transform t in rotatingObjects) {
			t.Rotate(Random.insideUnitSphere, Time.deltaTime * 360.0f);
		}
	}
}
