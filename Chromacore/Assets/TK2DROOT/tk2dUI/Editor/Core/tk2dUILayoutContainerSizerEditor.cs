using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(tk2dUILayoutContainerSizer))]
public class tk2dUILayoutContainerSizerEditor : tk2dUILayoutContainerEditor {
	new void OnEnable() {
		base.OnEnable();
		UpdateChildren();
	}

	bool allActive = false;
	bool hasNonLayouts = false;

	void UpdateChildren() {
		tk2dUILayoutContainerSizer sizer = (tk2dUILayoutContainerSizer)target;
		Transform t = sizer.transform;

		allActive = true;

		for (int i = 0; i < t.childCount; ++i) {
			Transform c = t.GetChild(i);
			tk2dUILayout layout = c.GetComponent<tk2dUILayout>();

			if (layout == null) {
				hasNonLayouts = true;
				break;
			}
		}

		foreach (tk2dUILayoutItem item in itemsList) {
			if (!item.inLayoutList) {
				allActive = false;
			}
		}
	}

	void AddMissingLayoutChildren(bool fixedSize) {
		tk2dUILayoutContainerSizer sizer = (tk2dUILayoutContainerSizer)target;
		foreach (tk2dUILayoutItem item in itemsList) {
			if (!item.inLayoutList) {
				item.inLayoutList = true;
				item.fixedSize = fixedSize;
				item.sizeProportion = 1;
				sizer.layoutItems.Add( item );
			}
		}
	}

	public override void OnInspectorGUI() {
		var sizer = (tk2dUILayoutContainerSizer)target;
		EditorGUIUtility.LookLikeControls();

		GUILayout.Space(8);

		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Direction");
		sizer.horizontal = GUILayout.Toolbar(sizer.horizontal ? 0 : 1, new string[] { "Horizontal", "Vertical" }) == 0;
		if (GUILayout.Button("R")) {
			GUI.changed = true;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		sizer.expand = GUILayout.Toggle( sizer.expand, sizer.horizontal ? "Expand Vertical" : "Expand Horizontal" );
		GUILayout.EndVertical();

		sizer.margin = EditorGUILayout.Vector2Field("Margin", sizer.margin);
		sizer.spacing = EditorGUILayout.FloatField("Spacing", sizer.spacing);
		GUILayout.EndVertical();

		if (!allActive) {
			int addMode = tk2dGuiUtility.InfoBoxWithButtons("Not all children UI Layouts are included in the sizer.", tk2dGuiUtility.WarningLevel.Error, "Fixed Size", "Proportional");
			if (addMode != -1) {
				AddMissingLayoutChildren( (addMode == 0) );
				UpdateChildren();
				GUI.changed = true;
			}
		}
		else if (hasNonLayouts) {
			EditorGUILayout.HelpBox("The sizer contains non UI Layout children. These will not be affected by the sizer.", MessageType.Info);
		}

		base.OnInspectorGUI();

		if (GUI.changed) {
			sizer.Refresh();
			Object[] objs = EditorUtility.CollectDeepHierarchy( new Object[] { target } );
			foreach (Object obj in objs) {
				EditorUtility.SetDirty( obj );
			}
		}
	}

	protected override void ItemInspector(tk2dUILayoutItem item) {
		var layout = item.layout;
		var sizer = (tk2dUILayoutContainerSizer)target;

		int selection = 0;
		if (item.fixedSize) selection = 1;
		else if (item.fillPercentage > 0) selection = 2;
		else if (!item.inLayoutList) selection = 0;
		else selection = 3;

		GUILayout.Label("Layout Mode");
		int newSelection = GUILayout.Toolbar(selection, new string[] { "Off", "Fixed Size", "Fill %", "Proportion" });
		if (newSelection != selection) {
			switch (newSelection) {
				case 0: 
					SetItemInLayoutList(item, false); 
					break;
				case 1: 
					SetItemInLayoutList(item, true); 
					item.fixedSize = true;
					item.fillPercentage = -1; 
					break;
				case 2: 
					item.fillPercentage = sizer.horizontal ? (100 * (layout.bMax.x - layout.bMin.x) / (sizer.bMax.x - sizer.bMin.x)) :
															 (100 * (layout.bMax.y - layout.bMin.y) / (sizer.bMax.y - sizer.bMin.y));
					SetItemInLayoutList(item, true); 
					item.fixedSize = false; 
					break;
				case 3:
					SetItemInLayoutList(item, true);
					item.fixedSize = false;
					item.fillPercentage = -1;
					item.sizeProportion = 1;
					break;
			}
		}

		EditorGUI.indentLevel++;
		if (selection == 2) {
			item.fillPercentage = EditorGUILayout.FloatField(sizer.horizontal ? "Width %" : "Height %", item.fillPercentage);
		}
		else if (selection == 3) {
			item.sizeProportion = EditorGUILayout.FloatField("Proportion", item.sizeProportion);
		}
		EditorGUI.indentLevel--;

		UpdateChildren();
	}
}
