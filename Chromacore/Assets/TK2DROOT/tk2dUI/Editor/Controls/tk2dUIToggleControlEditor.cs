using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIToggleControl))]
public class tk2dUIToggleControlEditor : tk2dUIToggleButtonEditor
{
    protected override void DrawGUI()
    {
        base.DrawGUI();

        tk2dUIToggleControl toggleBtn = (tk2dUIToggleControl)target;
        toggleBtn.descriptionTextMesh = EditorGUILayout.ObjectField("Description Text Mesh", toggleBtn.descriptionTextMesh, typeof(tk2dTextMesh), true) as tk2dTextMesh;
    }

}