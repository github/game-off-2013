using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class tk2dSceneHelper {
	// Positive rect
	public static Rect PositiveRect(Rect r) {
		if (r.width < 0.0f) r = new Rect(r.xMax, r.yMin, -r.width, r.height);
		if (r.height < 0.0f) r = new Rect(r.xMin, r.yMax, r.width, -r.height);
		return r;
	}

	// Misc drawing
	public static void DrawRect( Rect r, Transform t ) {
		Vector3 p0 = t.TransformPoint (new Vector3(r.xMin, r.yMin, 0));
		Vector3 p1 = t.TransformPoint (new Vector3(r.xMax, r.yMin, 0));
		Vector3 p2 = t.TransformPoint (new Vector3(r.xMax, r.yMax, 0));
		Vector3 p3 = t.TransformPoint (new Vector3(r.xMin, r.yMax, 0));
		Vector3[] worldPts = new Vector3[5] { p0, p1, p2, p3, p0 };
		Handles.DrawPolyLine (worldPts);
	}

	// Handle dragging sprite positions
	private static List<Transform> dragObjCachedTransforms = new List<Transform>();
	private static List<Vector3> dragObjStartPos = new List<Vector3>();
	private static Vector3 dragOrigin = Vector3.zero;
	private static Plane dragPlane = new Plane();
	public static void HandleMoveSprites(Transform t, Rect rect) {
		Event ev = Event.current;
		
		// Only enable in View mode, if requested
		if (!tk2dPreferences.inst.enableMoveHandles && Tools.current != Tool.View) {
			return;
		}

		// Break out if vertex modifier is active
		if (IsSnapToVertexActive()) {
			return;
		}

		int controlId = t.GetInstanceID();
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
		rect = PositiveRect(rect);

		if (ev.type == EventType.MouseDown && ev.button == 0 && !ev.control && !ev.alt && !ev.command) {
			float hitD = 0.0f;
			dragPlane = new Plane(t.forward, t.position);
			if (dragPlane.Raycast(mouseRay, out hitD)) {
				Vector3 intersect = mouseRay.GetPoint(hitD);
				Vector3 pLocal = t.InverseTransformPoint (intersect);

				if (pLocal.x >= rect.xMin && pLocal.x <= rect.xMax &&
				    pLocal.y >= rect.yMin && pLocal.y <= rect.yMax) {
					// Mousedown on our sprite

					// Store current selected objects transforms
					dragObjCachedTransforms.Clear();
					dragObjStartPos.Clear();
					for (int i = 0; i < Selection.gameObjects.Length; ++i) {
						Transform objTransform = Selection.gameObjects[i].transform;
						dragObjCachedTransforms.Add(objTransform);
						dragObjStartPos.Add(objTransform.position);
					}
					dragOrigin = intersect;

					GUIUtility.hotControl = controlId;
					ev.Use();
				}
			}
		}
		if (GUIUtility.hotControl == controlId) { // Handle drag / mouseUp
			switch (ev.GetTypeForControl(controlId)) {
				case EventType.MouseDrag: {
					float hitD = 0.0f;
					if (dragPlane.Raycast(mouseRay, out hitD)) {
						Vector3 intersect = mouseRay.GetPoint(hitD);
						Vector3 offset = intersect - dragOrigin;
						if (ev.shift) {
							float x = Mathf.Abs (Vector3.Dot (offset, t.right));
							float y = Mathf.Abs (Vector3.Dot (offset, t.up));
							offset = Vector3.Project (offset, (x > y) ? t.right : t.up);
						}

						Undo.RegisterUndo(dragObjCachedTransforms.ToArray(), "Move");

						for (int i = 0; i < Selection.gameObjects.Length; ++i) {
							Selection.gameObjects[i].transform.position = dragObjStartPos[i] + offset;
						}
					}
					break;
				}

				case EventType.MouseUp: {
					GUIUtility.hotControl = 0;
					ev.Use();
					break;
				}
			}
		}
	}
	
	static Vector2 mouseDownPos = Vector2.zero;

	// Handle selecting other sprites
	public static void HandleSelectSprites() {
		Event ev = Event.current;
		if (Tools.current == Tool.View) {
			if (ev.type == EventType.MouseDown && ev.button == 0) {
				mouseDownPos = ev.mousePosition;
			}
			if (ev.type == EventType.MouseUp && ev.button == 0 && ev.mousePosition == mouseDownPos) {

				bool changedSelection = false;

				List<Object> gos = new List<Object>(Selection.objects);
				Object go = HandleUtility.PickGameObject(ev.mousePosition, false);
				if (go != null) {
					if (ev.shift) {
						if (gos.Contains (go)) {
							gos.Remove (go);
						}
						else {
							gos.Add (go);
						}
						changedSelection = true;
					}
					else {
						if (!gos.Contains (go)) {
							gos.Clear ();
							gos.Add (go);
							changedSelection = true;
						}
					}
				}
				else {
					if (!ev.shift) {
						gos.Clear ();
						changedSelection = true;
					}
				}
				if (changedSelection) {
					Selection.objects = gos.ToArray();
					ev.Use ();
				}
			}
		}
	}

	// Are we enabling resize controls, or rotate controls?
	public static bool RectControlsToggle() {
		bool result = true;
		if (Event.current.alt) result = !result;
		if (Tools.current == Tool.Rotate) result = !result;
		return result;
	}

	// For constrain proportions
	private static Rect constrainRectTemp = new Rect();
	private static Rect constrainRect = new Rect();
	private static Matrix4x4 constrainRectMatrixTemp = Matrix4x4.zero;
	private static Matrix4x4 constrainRectMatrix = Matrix4x4.zero;

	// A draggable point
	public static Vector3 MoveHandle( int id, Vector3 worldPos, Vector3 planeNormal, GUIStyle style, MouseCursor cursor) {
		// If handle is behind camera,
		SceneView sceneview = SceneView.lastActiveSceneView;
		if (sceneview != null) {
			Camera sceneCam = sceneview.camera;
			if (sceneCam != null) {
				Vector3 camSpace = sceneCam.transform.InverseTransformPoint(worldPos);
				if (camSpace.z < 0.0f) {
					// then don't do this MoveHandle
					return worldPos;
				}
			}
		}

		Event ev = Event.current;
		Vector2 guiPoint = HandleUtility.WorldToGUIPoint( worldPos );

		int handleSize = (int)style.fixedWidth;
		bool selected = GUIUtility.hotControl == id;
		Rect handleRect = new Rect(guiPoint.x - handleSize / 2, guiPoint.y - handleSize / 2, handleSize, handleSize);
		EditorGUIUtility.AddCursorRect(handleRect, cursor);
		
		if (ev.type == EventType.Repaint) {
			style.Draw(handleRect, selected, false, false, false);
		}
		
		if (ev.type == EventType.MouseDown && ev.button == 0 && handleRect.Contains(ev.mousePosition)) {
			constrainRect = constrainRectTemp;
			constrainRectMatrix = constrainRectMatrixTemp;
			GUIUtility.hotControl = id;
			ev.Use();
		}
		else if (GUIUtility.hotControl == id) {
			switch (ev.GetTypeForControl(id)) {
				case EventType.MouseDrag: {
					Plane p = new Plane(planeNormal, worldPos);
					Ray r = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
					float d = 0;
					if (p.Raycast(r, out d)) {
						Vector3 hitPoint = r.GetPoint(d);
						GUI.changed = true;
						worldPos = hitPoint;
					}
					ev.Use();
					break;
				}
				case EventType.MouseUp: {
					GUIUtility.hotControl = 0;
					ev.Use();
					break;
				}
			}
		}
		return worldPos;
	}

	// Cursor stuff
	private static MouseCursor GetHandleCursor(Vector2 n, Transform objT) {
		n.Normalize ();
		Vector3 worldN = new Vector3(n.x, n.y, 0);
		worldN = objT.TransformDirection (worldN);
		worldN = Vector3.Scale (worldN, objT.localScale);
		Vector3 screenN = worldN;

		bool useSceneCam = true;
		if (useSceneCam) {
			SceneView sceneview = SceneView.lastActiveSceneView;
			if (sceneview != null) {
				Camera sceneCam = sceneview.camera;
				if (sceneCam != null) {
					screenN = sceneCam.transform.InverseTransformDirection(screenN);
				}
			}
		}

		Vector2[] cursorVec = new Vector2[] {
			new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f),
			new Vector2(1.0f, 1.0f), new Vector2(-1.0f, 1.0f)
		};
		MouseCursor[] cursors = new MouseCursor[] {
			MouseCursor.ResizeHorizontal, MouseCursor.ResizeVertical,
			MouseCursor.ResizeUpRight, MouseCursor.ResizeUpLeft
		};
		float maxDP = 0.0f;
		int maxInd = 0;
		for (int i = 0; i < 4; ++i) {
			Vector2 v = cursorVec[i];
			v.Normalize ();
			float dp = Mathf.Abs (v.x * screenN.x + v.y * screenN.y);
			if (dp > maxDP) {
				maxDP = dp;
				maxInd = i;
			}
		}
		return cursors[maxInd];
	}

	const float handleClosenessClip = 10.0f; // Don't draw handles when the rect gets this thin (screenspace)
	static bool vertexMoveModifierDown = false;

	private static bool IsSnapToVertexActive() {
		Event ev = Event.current;
		if (ev.isKey && ev.keyCode == KeyCode.V) {
			if (ev.type == EventType.KeyDown) vertexMoveModifierDown = true;
			else if (ev.type == EventType.KeyUp) vertexMoveModifierDown = false;
		}
		return (Tools.current == Tool.Move && vertexMoveModifierDown);
	}

	// 8 draggable points around the border (resizing)
	public static Rect RectControl( int controlId, Rect r, Transform t ) {
		Event ev = Event.current;

		// Break out if vertex modifier is active
		if (IsSnapToVertexActive()) {
			return r;
		}

		bool guiChanged = false;
		GUIStyle style = tk2dEditorSkin.MoveHandle;

		Vector2 rSign = new Vector2(Mathf.Sign (r.width), Mathf.Sign (r.height));
		r = PositiveRect(r);

		constrainRectTemp = r;
		constrainRectMatrixTemp = t.localToWorldMatrix;
		
		Vector3[] localPts = new Vector3[] {
			new Vector3(r.xMin + r.width * 0.0f, r.yMin + r.height * 0.0f, 0),
			new Vector3(r.xMin + r.width * 0.5f, r.yMin + r.height * 0.0f, 0),
			new Vector3(r.xMin + r.width * 1.0f, r.yMin + r.height * 0.0f, 0),
			new Vector3(r.xMin + r.width * 0.0f, r.yMin + r.height * 0.5f, 0),
			new Vector3(r.xMin + r.width * 1.0f, r.yMin + r.height * 0.5f, 0),
			new Vector3(r.xMin + r.width * 0.0f, r.yMin + r.height * 1.0f, 0),
			new Vector3(r.xMin + r.width * 0.5f, r.yMin + r.height * 1.0f, 0),
			new Vector3(r.xMin + r.width * 1.0f, r.yMin + r.height * 1.0f, 0),
		};

		Vector3[] worldPts = new Vector3[8];
		Vector2[] guiPts = new Vector2[8];
		bool[] handleVisible = new bool[8];
		for (int i = 0; i < 8; ++i) {
			worldPts[i] = t.TransformPoint(localPts[i]);
			guiPts[i] = HandleUtility.WorldToGUIPoint(worldPts[i]);
			handleVisible[i] = true;
		}

		// Hide handles if screen distance gets too small
		{
			float edgeLengthBottom = (guiPts[0] - guiPts[2]).magnitude;
			float edgeLengthTop = (guiPts[5] - guiPts[7]).magnitude;
			float edgeLengthLeft = (guiPts[0] - guiPts[5]).magnitude;
			float edgeLengthRight = (guiPts[2] - guiPts[7]).magnitude;
			if (edgeLengthBottom < handleClosenessClip || edgeLengthTop < handleClosenessClip ||
				edgeLengthLeft < handleClosenessClip || edgeLengthRight < handleClosenessClip)
			{
				for (int i = 0; i < 8; ++i) {
					handleVisible[i] = false;
				}
			}
			else {
				if (edgeLengthBottom < 2.0f * handleClosenessClip || edgeLengthTop < 2.0f * handleClosenessClip) {
					handleVisible[1] = handleVisible[6] = false;
				}
				if (edgeLengthLeft < 2.0f * handleClosenessClip || edgeLengthRight < 2.0f * handleClosenessClip) {
					handleVisible[3] = handleVisible[4] = false;
				}
			}
		}
		
		Vector2[] handleCursorN = new Vector2[] {
			new Vector2(-1.0f, -1.0f), new Vector2(0.0f, -1.0f), new Vector2(1.0f, -1.0f),
			new Vector2(-1.0f, 0.0f), new Vector2(1.0f, 0.0f),
			new Vector2(-1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f)
		};
		
		for (int i = 0; i < 8; ++i) {
			if ((Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDown) && !handleVisible[i]) continue;

			Vector3 worldPt = worldPts[i];
			MouseCursor cursor = GetHandleCursor(handleCursorN[i], t);
			
			EditorGUI.BeginChangeCheck();
			Vector3 newWorldPt = MoveHandle( controlId + t.GetInstanceID() + "Handle".GetHashCode() + i, worldPt, t.forward, style, cursor );
			if (EditorGUI.EndChangeCheck()) {
				Vector3 localPt = ev.shift ? constrainRectMatrix.inverse.MultiplyPoint(newWorldPt) : t.InverseTransformPoint(newWorldPt);
				Vector3 v0 = new Vector3(r.xMin, r.yMin, 0);
				Vector3 v1 = v0 + new Vector3(r.width, r.height, 0);
				
				// constrain axis
				if (i == 3 || i == 4) localPt.y = localPts[i].y;
				if (i == 1 || i == 6) localPt.x = localPts[i].x;
				
				// calculate new extrema
				if (!ev.shift) {
					if (i == 0 || i == 3 || i == 5) v0.x = Mathf.Min(v1.x, localPt.x);
					if (i == 0 || i == 1 || i == 2) v0.y = Mathf.Min(v1.y, localPt.y);
					if (i == 2 || i == 4 || i == 7) v1.x = Mathf.Max(v0.x, localPt.x);
					if (i == 5 || i == 6 || i == 7) v1.y = Mathf.Max(v0.y, localPt.y);
				} else {
					// constrain proportions
					v0 = new Vector3(constrainRect.xMin, constrainRect.yMin, 0);
					v1 = v0 + new Vector3(constrainRect.width, constrainRect.height, 0);
					if (i == 0 || i == 3 || i == 5) {
						v0.x = Mathf.Min(v1.x, localPt.x);
						float sy0 = (i == 0) ? 1.0f : ((i == 3) ? 0.5f : 0.0f);
						float dy = constrainRect.height * ((v1.x - v0.x) / constrainRect.width - 1.0f);
						v0.y -= dy * sy0;
						v1.y += dy * (1.0f - sy0);
					}
					if (i == 2 || i == 4 || i == 7) {
						v1.x = Mathf.Max(v0.x, localPt.x);
						float sy0 = (i == 2) ? 1.0f : ((i == 4) ? 0.5f : 0.0f);
						float dy = constrainRect.height * ((v1.x - v0.x) / constrainRect.width - 1.0f);
						v0.y -= dy * sy0;
						v1.y += dy * (1.0f - sy0);
					}
					if (i == 1 || i == 6) {
						if (i == 1) v0.y = Mathf.Min(v1.y, localPt.y);
						else v1.y = Mathf.Max(v0.y, localPt.y);
						float dx = constrainRect.width * ((v1.y - v0.y) / constrainRect.height - 1.0f);
						v0.x -= dx * 0.5f;
						v1.x += dx * 0.5f;
					}

					v0 = constrainRectMatrix.MultiplyPoint(v0);
					v1 = constrainRectMatrix.MultiplyPoint(v1);
					v0 = t.InverseTransformPoint(v0);
					v1 = t.InverseTransformPoint(v1);
				}
				
				guiChanged = true;
				r.Set(v0.x, v0.y, v1.x - v0.x, v1.y - v0.y);
				HandleUtility.Repaint();
			}
		}
		
		if (guiChanged) {
			GUI.changed = true;
		}

		if (rSign.x < 0.0f) r = new Rect(r.xMax, r.yMin, -r.width, r.height);
		if (rSign.y < 0.0f) r = new Rect(r.xMin, r.yMax, r.width, -r.height);
		return r;
	}

	// A few draggable corner points (rotation), returns change in angle (around 0,0 local to Rect)
	public static float RectRotateControl( int controlId, Rect r, Transform t, List<int> hideCornerPts) {
		Event ev = Event.current;
		bool guiChanged = false;

		GUIStyle style = tk2dEditorSkin.RotateHandle;

		r.xMin *= t.localScale.x;
		r.yMin *= t.localScale.y;
		r.xMax *= t.localScale.x;
		r.yMax *= t.localScale.y;
		Vector3 cachedScale = t.localScale;
		t.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		r = PositiveRect(r);

		Vector2[] corners = {	new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
								new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f)};

		Vector2[] handleCursorN = new Vector2[] {
			new Vector2(1.0f, -1.0f), new Vector2(1.0f, 1.0f),
			new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f)
		};

		Vector3[] worldPts = new Vector3[4];
		Vector2[] guiPts = new Vector2[4];
		for (int i = 0; i < 4; ++i) {
			Vector3 p = new Vector3( r.xMin + r.width * corners[i].x, r.yMin + r.height * corners[i].y, 0);
			worldPts[i] = t.TransformPoint (p);
			guiPts[i] = HandleUtility.WorldToGUIPoint(worldPts[i]);
		}
		
		// Exit early if handle screen distance is too small
		{
			float edgeLengthBottom = (guiPts[0] - guiPts[1]).magnitude;
			float edgeLengthLeft = (guiPts[0] - guiPts[2]).magnitude;
			if (edgeLengthBottom < handleClosenessClip || edgeLengthLeft < handleClosenessClip) {
				return 0.0f; // no rotation
			}
		}

		float result = 0.0f;
		for (int i = 0; i < 4; ++i) {
			if (!hideCornerPts.Contains (i)) {
				Vector3 p = new Vector3( r.xMin + r.width * corners[i].x, r.yMin + r.height * corners[i].y, 0);
				Vector3 worldPt = worldPts[i];
				MouseCursor cursor = GetHandleCursor(handleCursorN[i], t);
				EditorGUI.BeginChangeCheck();
				Vector3 newWorldPt = MoveHandle(controlId + t.GetInstanceID() + "Rotate".GetHashCode() + i, worldPt, t.forward, style, cursor);
				if (EditorGUI.EndChangeCheck()) {
					Vector3 d0 = p;
					Vector3 d1 = t.InverseTransformPoint (newWorldPt);
					d0.Normalize();
					d1.Normalize();
					float ang = Mathf.Acos(Vector3.Dot(d0, d1)) * Mathf.Rad2Deg;
					float sgn = Mathf.Sign(d1.x * -d0.y + d1.y * d0.x);
					result = ang * sgn;

					guiChanged = true;
					HandleUtility.Repaint();
				}
			}
		}

		t.localScale = cachedScale;

		if (ev.shift) {
			float snapAngle = 9.0f;
			result = (float)((int)(result / snapAngle)) * snapAngle;
		}

		if (guiChanged) {
			GUI.changed = true;
		}
		return result;
	}

	public static List<int> getAnchorHidePtList(tk2dBaseSprite.Anchor anchor, Rect r, Transform t) {
		List<int> hidePts = new List<int>();
		int x = 0;
		int y = 0;
		switch (anchor) {
			case tk2dBaseSprite.Anchor.LowerLeft: x = -1; y = -1; break;
			case tk2dBaseSprite.Anchor.LowerRight: x = 1; y = -1; break;
			case tk2dBaseSprite.Anchor.UpperLeft: x = -1; y = 1; break;
			case tk2dBaseSprite.Anchor.UpperRight: x = 1; y = 1; break;
		}
		if (r.width < 0.0f) x = -x;
		if (r.height < 0.0f) y = -y;
		if (t.localScale.x < 0.0f) x = -x;
		if (t.localScale.y < 0.0f) y = -y;
		if (x == -1 && y == -1) hidePts.Add (0);
		if (x == 1 && y == -1) hidePts.Add (1);
		if (x == -1 && y == 1) hidePts.Add (2);
		if (x == 1 && y == 1) hidePts.Add (3);
		return hidePts;
	}

	// Rect origin as offset from Anchor
	public static Vector2 GetAnchorOffset( Vector2 rectSize, tk2dBaseSprite.Anchor anchor ) {
		Vector2 anchorOffset = Vector3.zero;
		switch (anchor)
		{
			case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.UpperLeft: 
				break;
			case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.UpperCenter: 
				anchorOffset.x = -(rectSize.x / 2.0f); break;
			case tk2dBaseSprite.Anchor.LowerRight: case tk2dBaseSprite.Anchor.MiddleRight: case tk2dBaseSprite.Anchor.UpperRight: 
				anchorOffset.x = -(rectSize.x); break;
		}
		switch (anchor)
		{
			case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.LowerRight:
				break;
			case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.MiddleRight:
				anchorOffset.y = -(rectSize.y / 2.0f); break;
			case tk2dBaseSprite.Anchor.UpperLeft: case tk2dBaseSprite.Anchor.UpperCenter: case tk2dBaseSprite.Anchor.UpperRight:
				anchorOffset.y = -(rectSize.y); break;
		}
		return anchorOffset;
	}
}
