using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dCameraAnchor))]
public class tk2dCameraAnchorEditor : Editor 
{
	static string GetAnchorPointName( tk2dBaseSprite.Anchor anchor ) {
		return "Anchor (" + anchor.ToString() + ")";
	}
	public static void UpdateAnchorName(tk2dCameraAnchor anchor) {
		anchor.gameObject.name = GetAnchorPointName(anchor.AnchorPoint);
	}


	void OnDestroy() {
		tk2dEditorSkin.Done();
	}

	public override void OnInspectorGUI()
	{
		tk2dCameraAnchor _target = (tk2dCameraAnchor)this.target;

		tk2dBaseSprite.Anchor prevAnchorPoint = _target.AnchorPoint;
		_target.AnchorCamera = EditorGUILayout.ObjectField("Camera", _target.AnchorCamera, typeof(Camera), true) as Camera;
		_target.AnchorPoint = (tk2dBaseSprite.Anchor)EditorGUILayout.EnumPopup("Anchor Point", _target.AnchorPoint);

		if (_target.AnchorCamera != null && _target.AnchorCamera.GetComponent<tk2dCamera>() != null) {
			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Offset");
			Vector2 anchorOffset = _target.AnchorOffsetPixels;
			anchorOffset.x = EditorGUILayout.FloatField(anchorOffset.x, GUILayout.MaxWidth(60));
			anchorOffset.y = EditorGUILayout.FloatField(anchorOffset.y, GUILayout.MaxWidth(60));
			_target.AnchorOffsetPixels = anchorOffset;
			GUILayout.EndHorizontal();

			_target.AnchorToNativeBounds = EditorGUILayout.Toggle("To Native Bounds", _target.AnchorToNativeBounds);

			EditorGUI.indentLevel--;
		}

		if (GUI.changed) {
			_target.ForceUpdateTransform();
			if (prevAnchorPoint != _target.AnchorPoint 
				&& _target.gameObject.name == GetAnchorPointName(prevAnchorPoint)) {
				UpdateAnchorName( _target );
			}
			EditorUtility.SetDirty(_target);
		}
	}


	// Create tk2dCamera menu item
    [MenuItem("GameObject/Create Other/tk2d/Camera Anchor", false, 14906)]
    static void DoCreateCameraAnchorObject()
	{
		if (Selection.activeGameObject == null || Selection.activeGameObject.camera == null) {
			EditorUtility.DisplayDialog(
				"Camera Anchor Error", 
				"You will need to select a camera before creating an anchor attached to it", 
				"Ok");
		}
		else {
			GameObject go = new GameObject("");
			go.transform.parent = Selection.activeGameObject.transform;
			go.transform.localPosition = new Vector3(0, 0, 10);
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			tk2dCameraAnchor anchor = go.AddComponent<tk2dCameraAnchor>();
			anchor.AnchorCamera = Selection.activeGameObject.camera;
			UpdateAnchorName(anchor);

			EditorGUIUtility.PingObject( go );
			Selection.activeGameObject = go;
		}
	}
}
