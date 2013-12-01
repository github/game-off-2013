using UnityEngine;
using UnityEditor;
using System.Collections;

//[CustomEditor(typeof(tk2dResource))]
public class tk2dResourceEditor : Editor
{
	new tk2dResource target { get { return base.target as tk2dResource; } }

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		target.objectReference = EditorGUILayout.ObjectField("Object", target.objectReference, typeof(Object), false);
	}
}
