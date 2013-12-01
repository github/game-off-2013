using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dUIMask))]
public class tk2dUIMaskEditor : Editor {
	public override void OnInspectorGUI() {
		tk2dUIMask mask = (tk2dUIMask)target;

		DrawDefaultInspector();
		if (GUI.changed) {
			mask.Build();
		}
	}

    public void OnSceneGUI()
    {
		if (tk2dPreferences.inst.enableSpriteHandles == false) return;

    	tk2dUIMask mask = (tk2dUIMask)target;
		Transform t = mask.transform;
		Vector3 anchorOffset = tk2dSceneHelper.GetAnchorOffset(mask.size, mask.anchor);
		Rect localRect = new Rect(anchorOffset.x, anchorOffset.y, mask.size.x, mask.size.y);

		// Draw rect outline
		Handles.color = new Color(1,1,1,0.5f);
		tk2dSceneHelper.DrawRect (localRect, t);

		Handles.BeginGUI ();
		// Resize handles
		if (tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck ();
			Rect resizeRect = tk2dSceneHelper.RectControl (999888, localRect, t);
			if (EditorGUI.EndChangeCheck ()) {
				Vector2 newDim = new Vector2(resizeRect.width, resizeRect.height);
				newDim.x = Mathf.Abs(newDim.x);
				newDim.y = Mathf.Abs(newDim.y);
				Undo.RegisterUndo (new Object[] {t, mask}, "Resize");
				if (newDim != mask.size) {
					mask.size = newDim;
					mask.Build();
					Vector2 newAnchorOffset = tk2dSceneHelper.GetAnchorOffset (new Vector2(resizeRect.width, resizeRect.height), mask.anchor);
					Vector3 toNewAnchorPos = new Vector3(resizeRect.xMin - newAnchorOffset.x, resizeRect.yMin - newAnchorOffset.y, 0);
					Vector3 newPosition = t.TransformPoint (toNewAnchorPos);
					if (newPosition != t.position) {
						t.position = newPosition;
					}

					EditorUtility.SetDirty(mask);
				}
			}
		}
		// Rotate handles
		if (!tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck();
			float theta = tk2dSceneHelper.RectRotateControl (888999, localRect, t, new List<int>());
			if (EditorGUI.EndChangeCheck()) {
				Undo.RegisterUndo (t, "Rotate");
				if (Mathf.Abs(theta) > Mathf.Epsilon) {
					t.Rotate(t.forward, theta, Space.World);
				}
			}
		}
		Handles.EndGUI ();

		// Sprite selecting
		tk2dSceneHelper.HandleSelectSprites();

		// Move targeted sprites
    	tk2dSceneHelper.HandleMoveSprites(t, localRect);
	}
}
