using UnityEngine;
using System.Collections;

public class tk2dUIDemo3Controller : tk2dUIBaseDemoController {

	public Transform perspectiveCamera;
	public Transform overlayInterface;
	Vector3 overlayRestPosition = Vector3.zero;
	public Transform instructions;

	IEnumerator Start() {
		overlayRestPosition = overlayInterface.position;
		HideOverlay();

		Vector3 instructionsRestPos = instructions.position;
		instructions.position = instructions.position + instructions.up * 10;
		StartCoroutine( coMove( instructions, instructionsRestPos, 1 ) );

		yield return new WaitForSeconds( 3 );
		StartCoroutine( coMove( instructions, instructionsRestPos - instructions.up * 10, 1 ) );
	}

	public void ToggleCase(tk2dUIToggleButton button) {
		float targetAngle = ( button.IsOn ) ? -66 : 0;
		StartCoroutine( coTweenAngle(button.transform, targetAngle, 0.5f) );
	}

	IEnumerator coRedButtonPressed() {
		StartCoroutine( coShake(perspectiveCamera, Vector3.one, Vector3.one, 1.0f ) );

		yield return new WaitForSeconds(0.3f);
		ShowOverlay();
	}

	void ShowOverlay() {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		overlayInterface.gameObject.SetActiveRecursively(true);
#else
		overlayInterface.gameObject.SetActive(true);
#endif
		Vector3 v = overlayRestPosition;
		v.y = -2.5f;
		overlayInterface.position = v;
		StartCoroutine( coMove(overlayInterface, overlayRestPosition, 0.15f) );
	}

	IEnumerator coHideOverlay() {
		Vector3 v = overlayRestPosition;
		v.y = -2.5f;
		yield return StartCoroutine( coMove(overlayInterface, v, 0.15f) );
		HideOverlay();
	}

	void HideOverlay() {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		overlayInterface.gameObject.SetActiveRecursively(false);
#else
		overlayInterface.gameObject.SetActive(false);
#endif
	}
}
