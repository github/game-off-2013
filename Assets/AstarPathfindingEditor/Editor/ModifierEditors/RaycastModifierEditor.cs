using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RaycastModifier))]
public class RaycastModifierEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();
		
		EditorGUI.indentLevel = 1;
		
		RaycastModifier ob = target as RaycastModifier;
		
		ob.useRaycasting = EditorGUILayout.Toggle (new GUIContent ("Use Physics Raycasting"),ob.useRaycasting);
		
		if (ob.useRaycasting) {
			EditorGUI.indentLevel++;
			
			EditorGUI.indentLevel--;
		}
		
		Color preCol = GUI.color;
		GUI.color *= new Color (1,1,1,0.5F);
		ob.Priority = EditorGUILayout.IntField (new GUIContent ("Priority","Higher priority modifiers are executed first\nAdjust this in Seeker-->Modifier Priorities"),ob.Priority);
		GUI.color = preCol;
		
	}
}
