using UnityEngine;
using System.Collections;

public class tk2dTileMapDemoCoin : MonoBehaviour {

	public tk2dSpriteAnimator animator = null;

	void Awake() {
		if (animator == null) {
			Debug.LogError("Coin - Assign animator in the inspector before proceeding.");
			this.enabled = false;
		}
		else {
			animator.enabled = false;
		}
	}

	void OnBecameInvisible() {
		if (animator.enabled) {
			animator.enabled = false;
		}
	}

	void OnBecameVisible() {
		if (!animator.enabled) {
			animator.enabled = true;
		}
	}
}
