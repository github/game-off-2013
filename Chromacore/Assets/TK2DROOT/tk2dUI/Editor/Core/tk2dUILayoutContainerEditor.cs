using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dUILayoutContainer))]
public class tk2dUILayoutContainerEditor : tk2dUILayoutEditor {
	tk2dUILayout My {
		get {return (tk2dUILayout)target;}
	}

	protected override void GetItems(Transform t) {
		tk2dUILayout objLayout = t.GetComponent<tk2dUILayout>();

		if (objLayout != null) {
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
			curItem.layout = objLayout;
			curItem.gameObj = t.gameObject;
		}

		if (objLayout == null) {
			for (int i = 0; i < t.childCount; ++i)
				GetItems(t.GetChild(i));
		}
	}

	public override void OnInspectorGUI() {
		GUILayout.BeginVertical();

		if (My.GetComponent<tk2dBaseSprite>() != null) {
			EditorGUILayout.HelpBox("Please remove Sprite from this Object\nin order to use Layout!", MessageType.Error);
			GUILayout.EndVertical();
			return;
		}

		//My.bMin = EditorGUILayout.Vector3Field("bMin", My.bMin);
		//My.bMax = EditorGUILayout.Vector3Field("bMax", My.bMax);

		GUILayout.Space(8);

		bool warnLayoutHasSprite = false;
		string warnLayoutName = "";

		int moveSel = 0;

		GUILayout.BeginHorizontal();
		GUILayout.Label("Items");
		if (selItem != null) {
			if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_up"), GUILayout.ExpandWidth(false))) moveSel = -1;
			if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_down"), GUILayout.ExpandWidth(false))) moveSel = 1;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical("box");
		foreach (var item in itemsList) {
			if (item.inLayoutList) {
				GUI.color = Color.green;

				if (item.layout != null && item.gameObj.GetComponent<tk2dBaseSprite>() != null) {
					warnLayoutHasSprite = true;
					warnLayoutName = item.gameObj.name;
				}
			} else {
				GUI.color = Color.white;
			}
			if (GUILayout.Toggle(item == selItem, item.gameObj.name, tk2dEditorSkin.SC_ListBoxItem, GUILayout.ExpandWidth(true))) {
				if (selItem != item) {
					EditorGUIUtility.PingObject( item.gameObj );
					SceneView.RepaintAll();
				}
				selItem = item;
			}
		}
		GUILayout.EndVertical();
		GUI.color = Color.white;

		if (selItem != null && moveSel != 0) {
			for (int i = 0; i < My.layoutItems.Count; ++i) {
				if (My.layoutItems[i] == selItem) {
					int moveTo = i + moveSel;
					if (moveTo >= 0 && moveTo < My.layoutItems.Count) {
						var tmp = My.layoutItems[i];
						My.layoutItems[i] = My.layoutItems[moveTo];
						My.layoutItems[moveTo] = tmp;
					}
					break;
				}
			}
			OrderItems();
			GUI.changed = true;
		}

		if (warnLayoutHasSprite) {
			EditorGUILayout.HelpBox("Child Layout with Sprite found. Cannot resize \"" + warnLayoutName + "\"", MessageType.Error);
		}

		ArrowKeyNav();

		GUILayout.Space(8);

		if (selItem != null)
			ItemInspector(selItem);

		GUILayout.EndVertical();
	}

	protected void SetItemInLayoutList(tk2dUILayoutItem item, bool value) {
		if (value && !My.layoutItems.Contains(item)) {
			item.fixedSize = true; // default to fixed size
			My.layoutItems.Add(item);
			item.inLayoutList = true;
			OrderItems();
		} else if (!value && My.layoutItems.Contains(item)) {
			My.layoutItems.Remove(item);
			item.inLayoutList = false;
		}
	}

	protected virtual void ItemInspector(tk2dUILayoutItem item) {
		bool newInLayoutList = GUILayout.Toggle(item.inLayoutList, "Active");

		if (newInLayoutList && !My.layoutItems.Contains(item)) {
			item.fixedSize = true; // default to fixed size
			My.layoutItems.Add(item);
			item.inLayoutList = true;
			OrderItems();
		} else if (!newInLayoutList && My.layoutItems.Contains(item)) {
			My.layoutItems.Remove(item);
			item.inLayoutList = false;
		}
	}
}
