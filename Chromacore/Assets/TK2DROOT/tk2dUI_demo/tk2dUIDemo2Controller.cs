using UnityEngine;
using System.Collections;

public class tk2dUIDemo2Controller : tk2dUIBaseDemoController {

	public tk2dUILayout windowLayout;

	Vector3[] rectMin = new Vector3[] {
		Vector3.zero,
		new Vector3(-0.8f, -0.7f, 0),
		new Vector3(-0.9f, -0.9f, 0),
		new Vector3(-1.0f, -0.9f, 0),
		new Vector3(-1.0f, -1.0f, 0),
		Vector3.zero,
	};
	Vector3[] rectMax = new Vector3[] {
		Vector3.one,
		new Vector3(0.8f, 0.7f, 0),
		new Vector3(0.9f, 0.9f, 0),
		new Vector3(0.6f, 0.7f, 0),
		new Vector3(1.0f, 1.0f, 0),
		Vector3.one,
	};
	int currRect = 0;
	bool allowButtonPress = true;

	void Start() {
		// Read the current window bounds
		rectMin[0] = windowLayout.GetMinBounds();
		rectMax[0] = windowLayout.GetMaxBounds();
	}

	IEnumerator NextButtonPressed() {
		if (!allowButtonPress) {
			yield break;
		}
		
		allowButtonPress = false;
	
		currRect = (currRect + 1) % rectMin.Length;
		Vector3 min = rectMin[currRect];
		Vector3 max = rectMax[currRect];
		yield return StartCoroutine( coResizeLayout( windowLayout, min, max, 0.15f ) );

		allowButtonPress = true;
	}

	void LateUpdate() {
		// Get screen extents		
		int last = rectMin.Length - 1;
		rectMin[last].Set(tk2dCamera.Instance.ScreenExtents.xMin, tk2dCamera.Instance.ScreenExtents.yMin, 0);
		rectMax[last].Set(tk2dCamera.Instance.ScreenExtents.xMax, tk2dCamera.Instance.ScreenExtents.yMax, 0);
	}
}
