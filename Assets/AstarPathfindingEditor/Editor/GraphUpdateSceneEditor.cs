using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GraphUpdateScene))]
public class GraphUpdateSceneEditor : Editor {
	
	public override void OnInspectorGUI () {
		
		GraphUpdateScene script = target as GraphUpdateScene;
		
		if (script.points == null) script.points = new Vector3[0];
		
		Vector3[] prePoints = script.points;
		DrawDefaultInspector ();
		EditorGUI.indentLevel = 1;
		
		if (prePoints != script.points) { script.RecalcConvex (); HandleUtility.Repaint (); }
		
		bool preConvex = script.convex;
		script.convex = EditorGUILayout.Toggle (new GUIContent ("Convex","Sets if only the convex hull of the points should be used or the whole polygon"),script.convex);
		if (script.convex != preConvex) { script.RecalcConvex (); HandleUtility.Repaint (); }
		
		script.minBoundsHeight = EditorGUILayout.FloatField (new GUIContent ("Min Bounds Height","Defines a minimum height to be used for the bounds of the GUO.\nUseful if you define points in 2D (which would give height 0)"), script.minBoundsHeight);
		script.applyOnStart = EditorGUILayout.Toggle ("Apply On Start",script.applyOnStart);
		script.applyOnScan = EditorGUILayout.Toggle ("Apply On Scan",script.applyOnScan);
		
		script.modifyWalkability = EditorGUILayout.Toggle ("Modify walkability",script.modifyWalkability);
		if (script.modifyWalkability) {
			EditorGUI.indentLevel++;
			script.setWalkability = EditorGUILayout.Toggle ("Walkability",script.setWalkability);
			EditorGUI.indentLevel--;
		}
		
		script.penaltyDelta = EditorGUILayout.IntField ("Penalty Delta",script.penaltyDelta);
		
		if (script.penaltyDelta	< 0) {
			GUILayout.Label ("Be careful when lowering the penalty. Negative penalties are not supported and will instead underflow and get really high.","HelpBox");
		}
		
		bool worldSpace = EditorGUILayout.Toggle (new GUIContent ("Use World Space","Specify coordinates in world space or local space. When using local space you can move the GameObject around and the points will follow"
		                                                          ), script.useWorldSpace);
		if (worldSpace != script.useWorldSpace) {
			script.ToggleUseWorldSpace ();
		}
		
		script.modifyTag = EditorGUILayout.Toggle (new GUIContent ("Modify Tags","Should the tags of the nodes be modified"),script.modifyTag);
		if (script.modifyTag) {
			EditorGUI.indentLevel++;
			script.setTag = EditorGUILayout.Popup ("Set Tag",script.setTag,AstarPath.FindTagNames ());
			EditorGUI.indentLevel--;
		}
		
		//GUI.color = Color.red;
		if (GUILayout.Button ("Tags can be used to restrict which units can walk on what ground. Click here for more info","HelpBox")) {
			Application.OpenURL (AstarPathEditor.GetURL ("tags"));
		}
		
		//GUI.color = Color.white;
		
		EditorGUILayout.Separator ();
		
		//GUILayout.Space (0);
		//GUI.Toggle (r,script.lockToY,"","Button");
		script.lockToY = EditorGUILayout.Toggle ("Lock to Y",script.lockToY);
		
		if (script.lockToY) {
			EditorGUI.indentLevel++;
			script.lockToYValue = EditorGUILayout.FloatField ("Lock to Y value",script.lockToYValue);
			EditorGUI.indentLevel--;
			script.LockToY ();
		}
		
		EditorGUILayout.Separator ();
		
		if (GUI.changed) {
			Undo.RegisterUndo (script,"Modify Settings on GraphUpdateObject");
			EditorUtility.SetDirty (target);
		}
		
		if (GUILayout.Button ("Clear all points")) {
			Undo.RegisterUndo (script,"Removed All Points");
			script.points = new Vector3[0];
			script.RecalcConvex ();
		}
		
	}
	
	int selectedPoint = -1;
	
	const float pointGizmosRadius = 0.09F;
	static Color PointColor = new Color (1,0.36F,0,0.6F);
	static Color PointSelectedColor = new Color (1,0.24F,0,1.0F);
	
	public void OnSceneGUI () {
		
		
		GraphUpdateScene script = target as GraphUpdateScene;
		
		if (script.points == null) script.points = new Vector3[0];
		List<Vector3> points = Pathfinding.Util.ListPool<Vector3>.Claim ();
		points.AddRange (script.points);
		
		Matrix4x4 invMatrix = script.useWorldSpace ? Matrix4x4.identity : script.transform.worldToLocalMatrix;
		
		if (!script.useWorldSpace) {
			Matrix4x4 matrix = script.transform.localToWorldMatrix;
			for (int i=0;i<points.Count;i++) points[i] = matrix.MultiplyPoint3x4(points[i]);
		}
		
		
		if (Tools.current != Tool.View && Event.current.type == EventType.Layout) {
			for (int i=0;i<script.points.Length;i++) {
				HandleUtility.AddControl (-i - 1,HandleUtility.DistanceToLine (points[i],points[i]));
			}
		}
		
		if (Tools.current != Tool.View)
			HandleUtility.AddDefaultControl (0);
		
		for (int i=0;i<points.Count;i++) {
			
			if (i == selectedPoint && Tools.current == Tool.Move) {
				Handles.color = PointSelectedColor;
				Undo.SetSnapshotTarget(script, "Moved Point");
				Handles.SphereCap (-i-1,points[i],Quaternion.identity,HandleUtility.GetHandleSize (points[i])*pointGizmosRadius*2);
				Vector3 pre = points[i];
				Vector3 post = Handles.PositionHandle (points[i],Quaternion.identity);
				if (pre != post) {
					script.points[i] = invMatrix.MultiplyPoint3x4(post);
				}
			} else {
				Handles.color = PointColor;
				Handles.SphereCap (-i-1,points[i],Quaternion.identity,HandleUtility.GetHandleSize (points[i])*pointGizmosRadius);
			}
		}
		
		if(Input.GetMouseButtonDown(0)) {
            // Register the undos when we press the Mouse button.
            Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
		
		if (Event.current.type == EventType.MouseDown) {
			int pre = selectedPoint;
			selectedPoint = -(HandleUtility.nearestControl+1);
			if (pre != selectedPoint) GUI.changed = true;
		}
		
		if (Event.current.type == EventType.MouseDown && Event.current.shift && Tools.current == Tool.Move) {
			
			if (((int)Event.current.modifiers & (int)EventModifiers.Alt) != 0) {
				//int nearestControl = -(HandleUtility.nearestControl+1);
				
				if (selectedPoint >= 0 && selectedPoint < points.Count) {
					Undo.RegisterUndo (script,"Removed Point");
					List<Vector3> arr = new List<Vector3>(script.points);
					arr.RemoveAt (selectedPoint);
					points.RemoveAt (selectedPoint);
					script.points = arr.ToArray ();
					script.RecalcConvex ();
					GUI.changed = true;
				}
			} else if (((int)Event.current.modifiers & (int)EventModifiers.Control) != 0 && points.Count > 1) {
				
				int minSeg = 0;
				float minDist = float.PositiveInfinity;
				for (int i=0;i<points.Count;i++) {
					float dist = HandleUtility.DistanceToLine (points[i],points[(i+1)%points.Count]);
					if (dist < minDist) {
						minSeg = i;
						minDist = dist;
					}
				}
				
				System.Object hit = HandleUtility.RaySnap (HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
				if (hit != null) {
					RaycastHit rayhit = (RaycastHit)hit;
					
					Undo.RegisterUndo (script,"Added Point");
					
					List<Vector3> arr = Pathfinding.Util.ListPool<Vector3>.Claim ();
					arr.AddRange (script.points);
					
					points.Insert (minSeg+1,rayhit.point);
					if (!script.useWorldSpace) rayhit.point = invMatrix.MultiplyPoint3x4 (rayhit.point);
					
					arr.Insert (minSeg+1,rayhit.point);
					script.points = arr.ToArray ();
					script.RecalcConvex ();
					Pathfinding.Util.ListPool<Vector3>.Release (arr);
					GUI.changed = true;
				}
			} else {
				System.Object hit = HandleUtility.RaySnap (HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
				if (hit != null) {
					RaycastHit rayhit = (RaycastHit)hit;
					
					Undo.RegisterUndo (script,"Added Point");
					
					Vector3[] arr = new Vector3[script.points.Length+1];
					for (int i=0;i<script.points.Length;i++) {
						arr[i] = script.points[i];
					}
					points.Add (rayhit.point);
					if (!script.useWorldSpace) rayhit.point = invMatrix.MultiplyPoint3x4 (rayhit.point);
					
					arr[script.points.Length] = rayhit.point;
					script.points = arr;
					script.RecalcConvex ();
					GUI.changed = true;
				}
			}
			Event.current.Use ();
		}
		
		if (Event.current.shift && Event.current.type == EventType.MouseDrag) {
			//Event.current.Use ();
		}
		
		//if (!script.useWorldSpace) {
		//	Matrix4x4 matrix = script.transform.worldToLocalMatrix;
		//	for (int i=0;i<points.Count;i++) points[i] = matrix.MultiplyPoint3x4(points[i]);
		//}
		
		Pathfinding.Util.ListPool<Vector3>.Release (points);
		
		if (GUI.changed) { HandleUtility.Repaint (); EditorUtility.SetDirty (target); }
		
	}
}
