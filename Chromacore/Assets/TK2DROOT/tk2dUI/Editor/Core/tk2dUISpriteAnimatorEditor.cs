using UnityEditor;
using UnityEngine;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUISpriteAnimator))]
public class tk2dUISpriteAnimatorEditor : tk2dSpriteAnimatorEditor {
	[MenuItem("CONTEXT/tk2dSpriteAnimator/Convert to UI Sprite Animator")]
	static void DoConvertUISpriteAnimator() {
		Undo.RegisterSceneUndo("Convert UI Sprite Animator");
		foreach (GameObject go in Selection.gameObjects) {
			tk2dSpriteAnimator animator = go.GetComponent<tk2dSpriteAnimator>();
			if (animator != null) {
				tk2dUISpriteAnimator UIanimator = go.AddComponent<tk2dUISpriteAnimator>();
				UIanimator.Library = animator.Library;
				UIanimator.DefaultClipId = animator.DefaultClipId;
				UIanimator.playAutomatically = animator.playAutomatically;
				DestroyImmediate(animator);
			}
		}
	}
}