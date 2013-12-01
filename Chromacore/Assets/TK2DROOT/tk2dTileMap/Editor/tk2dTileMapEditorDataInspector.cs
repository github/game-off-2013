using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(tk2dTileMapEditorData))]
public class tk2dTileMapEditorDataInspector : Editor 
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Label("Inspector disabled.\n" +
			"Please use debug mode to edit this object.");
	}
}
