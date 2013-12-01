using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dUILayout))]
public class tk2dUILayoutEditor : Editor {
	tk2dUILayout My {
		get {return (tk2dUILayout)target;}
	}

	bool updateChildren = true;

	void DrawLayoutOutline(Transform t) {
		var layout = t.GetComponent<tk2dUILayout>();
		if (layout != null) {
			Vector3[] p = new Vector3[] {
				new Vector3(layout.bMin.x, layout.bMin.y, 0.0f),
				new Vector3(layout.bMax.x, layout.bMin.y, 0.0f),
				new Vector3(layout.bMax.x, layout.bMax.y, 0.0f),
				new Vector3(layout.bMin.x, layout.bMax.y, 0.0f),
				new Vector3(layout.bMin.x, layout.bMin.y, 0.0f),
			};
			for (int i = 0; i < p.Length; ++i) p[i] = t.TransformPoint(p[i]);
			Handles.color = Color.magenta;
			Handles.DrawPolyLine(p);

			var sizer = t.GetComponent<tk2dUILayoutContainerSizer>();
			if (sizer != null) {
				Handles.color = Color.cyan;
				float arrowSize = 0.3f * HandleUtility.GetHandleSize(p[0]);
				if (sizer.horizontal) {
					Handles.ArrowCap(0, (p[0] + p[3]) * 0.5f, Quaternion.LookRotation(p[1] - p[0]), arrowSize);
					Handles.ArrowCap(0, (p[1] + p[2]) * 0.5f, Quaternion.LookRotation(p[0] - p[1]), arrowSize);
				} else {
					Handles.ArrowCap(0, (p[0] + p[1]) * 0.5f, Quaternion.LookRotation(p[3] - p[0]), arrowSize);
					Handles.ArrowCap(0, (p[2] + p[3]) * 0.5f, Quaternion.LookRotation(p[0] - p[3]), arrowSize);
				}
			}
		}

		for (int i = 0; i < t.childCount; ++i)
			DrawLayoutOutline(t.GetChild(i));
	}

	public void OnSceneGUI() {
		if (My.GetComponent<tk2dBaseSprite>() != null)
			return;

		Transform t = My.transform;

		DrawLayoutOutline(t);

		Rect r0 = new Rect(My.bMin.x, My.bMin.y, My.bMax.x - My.bMin.x, My.bMax.y - My.bMin.y);
		Handles.BeginGUI();
		Rect r1 = tk2dSceneHelper.RectControl("UILayout".GetHashCode(), r0, t);
		Handles.EndGUI();
		if (r0 != r1) {
			Vector3 dMin = new Vector3(r1.xMin - r0.xMin, r1.yMin - r0.yMin);
			Vector3 dMax = new Vector3(r1.xMax - r0.xMax, r1.yMax - r0.yMax);
			Object[] deps = EditorUtility.CollectDeepHierarchy(new Object[] {My});
			Undo.RegisterUndo(deps, "Resize");
			My.Reshape(dMin, dMax, updateChildren);
			foreach (var dep in deps)
				EditorUtility.SetDirty(dep);
		}

		Event ev = Event.current;
		if (ev.type == EventType.ValidateCommand && ev.commandName == "UndoRedoPerformed") {
			tk2dBaseSprite[] sprites = My.GetComponentsInChildren<tk2dBaseSprite>() as tk2dBaseSprite[];
			foreach (tk2dBaseSprite sprite in sprites) {
				sprite.ForceBuild();
			}
			tk2dTextMesh[] textMeshes = My.GetComponentsInChildren<tk2dTextMesh>() as tk2dTextMesh[];
			foreach (tk2dTextMesh textMesh in textMeshes) {
				textMesh.ForceBuild();
			}
		}

		// Draw outline of selected item
		if (selItem != null) {
			if (selItem.gameObj.renderer != null || selItem.layout != null) {
				float s = HandleUtility.GetHandleSize(t.position) * 0.05f;
				Vector3 svec = new Vector3(s, s);
				Bounds b = new Bounds();
				if (selItem.layout != null) {
					b.center = selItem.gameObj.transform.position + 0.5f * (selItem.layout.bMin + selItem.layout.bMax);
					b.size = selItem.layout.bMax - selItem.layout.bMin;
				} else {
					b = selItem.gameObj.renderer.bounds;
				}
				Handles.color = Color.red;
				float[] kx = new float[] {-1, 1, 1, -1, -1};
				float[] ky = new float[] {-1, -1, 1, 1, -1};
				for (int k = 0; k < 4; ++k) {
					Vector3 p1 = b.center + new Vector3(b.extents.x * kx[k], b.extents.y * ky[k]);
					Vector3 p2 = b.center + new Vector3(b.extents.x * kx[k + 1], b.extents.y * ky[k + 1]);
					int nLines = (int)((p1 - p2).magnitude / s);
					for (int i = 0; i < nLines; ++i) {
						Vector3 q = p1 + ((float)i / (float)nLines) * (p2 - p1);
						Handles.DrawLine(q - svec, q + svec);
					}
				}
			}
		}
	}

	protected List<tk2dUILayoutItem> itemsList = null;
	protected tk2dUILayoutItem selItem = null;

	protected virtual void GetItems(Transform t) {
		tk2dBaseSprite objSprite = t.GetComponent<tk2dBaseSprite>();
		tk2dUIMask objMask = t.GetComponent<tk2dUIMask>();
		tk2dUILayout objLayout = t.GetComponent<tk2dUILayout>();

		tk2dUILayoutItem curItem = null;
		foreach (var item in My.layoutItems) {
			if (t.gameObject == item.gameObj) {
				curItem = item;
				curItem.inLayoutList = true;
				break;
			}
		}
		if (curItem == null)
			curItem = new tk2dUILayoutItem();
		itemsList.Add(curItem);
		curItem.sprite = objSprite;
		curItem.UIMask = objMask;
		curItem.layout = objLayout;
		curItem.gameObj = t.gameObject;

		if (objLayout == null) {
			for (int i = 0; i < t.childCount; ++i)
				GetItems(t.GetChild(i));
		}
	}

	public void OnEnable() {
		foreach (var item in My.layoutItems)
			item.inLayoutList = false;

		itemsList = new List<tk2dUILayoutItem>();
		for (int i = 0; i < My.transform.childCount; ++i)
			GetItems(My.transform.GetChild(i));

		selItem = null;

		// Remove my items that weren't found in children
		List<tk2dUILayoutItem> removeItems = new List<tk2dUILayoutItem>();
		foreach (var item in My.layoutItems)
			if (!item.inLayoutList)
				removeItems.Add(item);
		foreach (var item in removeItems)
			My.layoutItems.Remove(item);

		OrderItems();
	}

	public override void OnInspectorGUI() {
		GUILayout.Space(16);
		GUILayout.BeginVertical();

		if (My.GetComponent<tk2dBaseSprite>() != null || My.GetComponent<tk2dTextMesh>() != null || My.GetComponent<tk2dUIMask>() != null) {
			EditorGUILayout.HelpBox("Please remove Sprite/TextMesh/UIMask from this Object\nin order to use Layout!", MessageType.Error);
			GUILayout.EndVertical();
			return;
		}

		//My.bMin = EditorGUILayout.Vector3Field("bMin", My.bMin);
		//My.bMax = EditorGUILayout.Vector3Field("bMax", My.bMax);

		GUILayout.BeginHorizontal();

		int width = 96;

		GUILayout.BeginVertical();
		updateChildren = !GUILayout.Toggle(!updateChildren, "Edit Mode", "button", GUILayout.ExpandWidth(false), GUILayout.Width(width));
		bool editMode = !updateChildren;
		GUI.enabled = editMode;

		EditorGUI.indentLevel++;
		My.autoResizeCollider = EditorGUILayout.Toggle("Resize Collider", My.autoResizeCollider);

		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Fit Layout");
		if (GUILayout.Button("To Geometry", GUILayout.ExpandWidth(false), GUILayout.Width(width))) {
			int numInList = 0;
			foreach (tk2dUILayoutItem v in itemsList) {
				if (v.inLayoutList) {
					numInList++;
				}
			}
			if (numInList == 0) {
				EditorUtility.DisplayDialog("Fit Layout", "Fit Layout requires items anchored in the layout.", "Ok");
			}
			else {
				Object[] deps = EditorUtility.CollectDeepHierarchy(new Object[] {My});
				Undo.RegisterUndo(deps, "Resize");

				Vector3[] minMax = new Vector3[] {Vector3.one * float.MaxValue, Vector3.one * -float.MaxValue};
				GetChildRendererBounds(My.transform.worldToLocalMatrix, minMax, My.transform);
				Vector3 dMin = new Vector3(minMax[0].x - My.bMin.x, minMax[0].y - My.bMin.y);
				Vector3 dMax = new Vector3(minMax[1].x - My.bMax.x, minMax[1].y - My.bMax.y);

				bool lastAutoResizeCollider = My.autoResizeCollider;
				Vector3 lastPos = My.transform.position;
				My.autoResizeCollider = false;
				My.Reshape(dMin, dMax, false);
				My.autoResizeCollider = lastAutoResizeCollider;
				var box = My.GetComponent<BoxCollider>();
				if (box != null)
					box.center -= My.transform.worldToLocalMatrix.MultiplyVector(My.transform.position - lastPos);

				foreach (var dep in deps)
					EditorUtility.SetDirty(dep);
			}
		}
		GUILayout.EndHorizontal();
		EditorGUI.indentLevel--;
		GUI.enabled = true;

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUI.enabled = editMode && (selItem != null);
		ItemInspector(selItem);
		GUI.enabled = true;

		GUILayout.EndHorizontal();

		GUILayout.Space(4);

		bool warnLayoutHasSprite = false;
		string warnLayoutName = "";

		GUILayout.BeginVertical("box");
		foreach (var item in itemsList) {
			if (item.inLayoutList) {
				if (item.layout) GUI.color = new Color(1.0f, 1.0f, 0.3f);
				else GUI.color = Color.green;

				if (item.layout != null && item.gameObj.GetComponent<tk2dBaseSprite>() != null) {
					warnLayoutHasSprite = true;
					warnLayoutName = item.gameObj.name;
				}
			} else {
				GUI.color = Color.white;
			}
			if (GUILayout.Toggle(item == selItem, item.gameObj.name, tk2dEditorSkin.SC_ListBoxItem)) {
				if (selItem != item) {
					EditorGUIUtility.PingObject( item.gameObj );
					SceneView.RepaintAll();
					Repaint();
				}
				selItem = item;
			}
		}
		GUILayout.EndVertical();
		GUI.color = Color.white;

		if (warnLayoutHasSprite) {
			EditorGUILayout.HelpBox("Child Layout with Sprite found. Cannot resize \"" + warnLayoutName + "\"", MessageType.Error);
		}

		ArrowKeyNav();

		GUILayout.Space(40);

		GUILayout.EndVertical();

		if (GUI.changed) {
			EditorUtility.SetDirty(target);
		}
	}

	void ItemInspector(tk2dUILayoutItem item) {
		float snapControlSize = 86;
		float snapButtonSize = 17;
		float snapButtonBorder = 1;
		float middleMarginSize = snapButtonSize + snapButtonBorder * 2;
		Event ev = Event.current;

		Color32 tmpGray = Color.gray;
		Color activeColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
		Color grayColor = EditorGUIUtility.isProSkin ? tmpGray : new Color32(150, 150, 150, 255);
		Color fillColor = EditorGUIUtility.isProSkin ? new Color32(136, 201, 27, 255) : new Color32(62, 115, 0, 255);

		GUILayout.BeginHorizontal();
		GUILayout.Space(8);
		Rect rect = GUILayoutUtility.GetRect(snapControlSize, snapControlSize, GUILayout.ExpandWidth(false));
		GUILayout.Space(4);
		GUILayout.EndHorizontal();

		GUIStyle borderStyle = tk2dEditorSkin.GetStyle("Border");
		GUI.color = activeColor; GUI.Box(rect, "", borderStyle); GUI.color = Color.white;
		
		Rect middleRect = new Rect( rect.x + middleMarginSize, rect.y + middleMarginSize, rect.width - middleMarginSize * 2, rect.height - middleMarginSize * 2 );
		GUI.color = EditorGUIUtility.isProSkin ? grayColor : Color.black;
		GUI.Box(middleRect, "", borderStyle);
		GUI.color = Color.white;

		GUIStyle style = new GUIStyle("");
		style.border = new RectOffset(0, 0, 0, 0);
		style.margin = new RectOffset(0, 0, 0, 0);
		style.padding = new RectOffset(0, 0, 0, 0);

		Rect r = new Rect(rect.x + snapButtonBorder, rect.y + snapButtonBorder, rect.width - snapButtonBorder * 2, rect.height - snapButtonBorder * 2);

		if (item == null) {
			GUI.color = grayColor;
			GUI.Toggle(new Rect(r.x, r.y + r.height / 2 - snapButtonSize / 2, snapButtonSize, snapButtonSize), false, tk2dEditorSkin.GetTexture("anchor_lr"), style);
			GUI.Toggle(new Rect(r.x + r.width - snapButtonSize, r.y + r.height / 2 - snapButtonSize / 2, snapButtonSize, snapButtonSize), false, tk2dEditorSkin.GetTexture("anchor_lr"), style);
			GUI.Toggle(new Rect(r.x + r.width / 2 - snapButtonSize / 2, r.y + r.height - snapButtonSize, snapButtonSize, snapButtonSize), false, tk2dEditorSkin.GetTexture("anchor_ud"), style);
			GUI.Toggle(new Rect(r.x + r.width / 2 - snapButtonSize / 2, r.y, snapButtonSize, snapButtonSize), false, tk2dEditorSkin.GetTexture("anchor_ud"), style);
			GUI.color = Color.white;
			return;
		}

		GUI.color = item.snapLeft ? activeColor : grayColor; item.snapLeft = GUI.Toggle(new Rect(r.x, r.y + r.height / 2 - snapButtonSize / 2, snapButtonSize, snapButtonSize), item.snapLeft, tk2dEditorSkin.GetTexture("anchor_lr"), style);
		GUI.color = item.snapRight ? activeColor : grayColor; item.snapRight = GUI.Toggle(new Rect(r.x + r.width - snapButtonSize, r.y + r.height / 2 - snapButtonSize / 2, snapButtonSize, snapButtonSize), item.snapRight, tk2dEditorSkin.GetTexture("anchor_lr"), style);
		GUI.color = item.snapBottom ? activeColor : grayColor; item.snapBottom = GUI.Toggle(new Rect(r.x + r.width / 2 - snapButtonSize / 2, r.y + r.height - snapButtonSize, snapButtonSize, snapButtonSize), item.snapBottom, tk2dEditorSkin.GetTexture("anchor_ud"), style);
		GUI.color = item.snapTop ? activeColor : grayColor; item.snapTop = GUI.Toggle(new Rect(r.x + r.width / 2 - snapButtonSize / 2, r.y, snapButtonSize, snapButtonSize), item.snapTop, tk2dEditorSkin.GetTexture("anchor_ud"), style);

		bool hasSnap = item.snapLeft || item.snapRight || item.snapBottom || item.snapTop;
		bool allSnap = item.snapLeft && item.snapRight && item.snapTop && item.snapBottom;

		if ( ev.type == EventType.MouseUp && middleRect.Contains(ev.mousePosition) ) {
			bool v = !allSnap;
			item.snapLeft = item.snapRight = item.snapTop = item.snapBottom = v;
			ev.Use();
		}

		int strectRectSize = 20;
		int strectRectBorder = 4;
		Rect fullStrectRect = new Rect( middleRect.x + strectRectBorder, middleRect.y + strectRectBorder, middleRect.width - strectRectBorder * 2, middleRect.height - strectRectBorder * 2);
		Rect strechRect = new Rect( fullStrectRect.x + fullStrectRect.width / 2 - strectRectSize / 2, fullStrectRect.y + fullStrectRect.height / 2 - strectRectSize / 2, strectRectSize, strectRectSize );
		if (item.snapLeft) strechRect.xMin = fullStrectRect.xMin;
		if (item.snapRight) strechRect.xMax = fullStrectRect.xMax;
		if (item.snapTop) strechRect.yMin = fullStrectRect.yMin;
		if (item.snapBottom) strechRect.yMax = fullStrectRect.yMax;
		GUI.color = hasSnap ? fillColor : grayColor;
		GUI.Box( strechRect, "", tk2dEditorSkin.WhiteBox );
		GUI.color = Color.white;

		if (hasSnap && !My.layoutItems.Contains(item)) {
			My.layoutItems.Add(item);
			item.inLayoutList = true;
			OrderItems();
		} else if (!hasSnap && My.layoutItems.Contains(item)) {
			My.layoutItems.Remove(item);
			item.inLayoutList = false;
		}
	}

	void SetItemsChildDepth(Transform t, int depth) {
		foreach (var item in itemsList) {
			if (t.gameObject == item.gameObj) {
				item.childDepth = depth;
				break;
			}
		}

		for (int i = 0; i < t.childCount; ++i)
			SetItemsChildDepth(t.GetChild(i), depth + 1);
	}

	void OrderItemsList(List<tk2dUILayoutItem> list) {
		for (int i = 0; i < list.Count; ++i) {
			for (int j = i + 1; j < list.Count; ++j) {
				var a = list[i];
				bool aHasLayout = a.gameObj.GetComponent<tk2dUILayout>() != null;
				var b = list[j];
				bool bHasLayout = b.gameObj.GetComponent<tk2dUILayout>() != null;
				bool swap = false;
				if (aHasLayout != bHasLayout) {
					swap = !aHasLayout;
				} else {
					if (a.inLayoutList != b.inLayoutList) {
						swap = !a.inLayoutList;
					} else {
						if (a.inLayoutList) {
							swap = (My.layoutItems.IndexOf(a) > My.layoutItems.IndexOf(b));
						} else {
							if (a.childDepth != b.childDepth) {
								swap = (a.childDepth > b.childDepth);
							} else {
								swap = (string.Compare(a.gameObj.name, b.gameObj.name) > 0);
							}
						}
					}
				}
				if (swap) {
					var tmp = list[i];
					list[i] = list[j];
					list[j] = tmp;
				}
			}
		}
	}

	protected void OrderItems() {
		for (int i = 0; i < My.transform.childCount; ++i)
			SetItemsChildDepth(My.transform.GetChild(i), 0);

		OrderItemsList(itemsList);
	}

	void GetChildRendererBounds(Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t) {
		MeshFilter mf = t.GetComponent<MeshFilter>();
		if (mf != null && mf.sharedMesh != null) {
			Matrix4x4 m = rootWorldToLocal * t.localToWorldMatrix;
			Vector3 basisX = m.MultiplyVector(new Vector3(1,0,0));
			Vector3 basisY = m.MultiplyVector(new Vector3(0,1,0));
			Vector3 basisZ = m.MultiplyVector(new Vector3(0,0,1));
			basisX = Vector3.Max(basisX, basisX * -1);
			basisY = Vector3.Max(basisY, basisY * -1);
			basisZ = Vector3.Max(basisZ, basisZ * -1);
			Bounds b = mf.sharedMesh.bounds;
			Vector3 c = m.MultiplyPoint(b.center);
			Vector3 d = basisX * b.extents.x + basisY * b.extents.y + basisZ * b.extents.z;
			minMax[0] = Vector3.Min(minMax[0], c - d);
			minMax[1] = Vector3.Max(minMax[1], c + d);
		}
		for (int i = 0; i < t.childCount; ++i)
			GetChildRendererBounds(rootWorldToLocal, minMax, t.GetChild(i));
	}

	protected void ArrowKeyNav() {
		Event ev = Event.current;
		if (ev.type == EventType.KeyDown && (ev.keyCode == KeyCode.DownArrow || ev.keyCode == KeyCode.UpArrow)) {
			int arrowkeySel = (ev.keyCode == KeyCode.DownArrow) ? 1 : -1;
			int selIdx = -1;
			for (int i = 0; i < itemsList.Count; ++i)
				if (itemsList[i] == selItem)
					selIdx = i;
			if (selIdx != -1) {
				selIdx += arrowkeySel;
				if (selIdx >= 0 && selIdx < itemsList.Count) {
					selItem = itemsList[selIdx];
					EditorGUIUtility.PingObject(selItem.gameObj);
					SceneView.RepaintAll();
				}
				ev.Use();
			}
		}
		if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape) {
			if (selItem != null) {
				selItem = null;
				ev.Use();
			}
		}
	}
}
