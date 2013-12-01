using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIToggleButton))]
public class tk2dUIToggleButtonEditor : tk2dUIBaseItemControlEditor
{
    protected virtual void DrawGUI() {
        tk2dUIToggleButton toggleBtn = (tk2dUIToggleButton)target;
        toggleBtn.onStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("On State GameObject", toggleBtn.onStateGO,target);
        toggleBtn.offStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Off State GameObject", toggleBtn.offStateGO,target);
        toggleBtn.activateOnPress = EditorGUILayout.Toggle("Activate On Press", toggleBtn.activateOnPress);
        toggleBtn.IsOn = EditorGUILayout.Toggle("Is On", toggleBtn.IsOn);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tk2dUIToggleButton toggleBtn = (tk2dUIToggleButton)target;

        DrawGUI();

        BeginMessageGUI();
        methodBindingUtil.MethodBinding( "On Toggle", typeof(tk2dUIToggleButton), toggleBtn.SendMessageTarget, ref toggleBtn.SendMessageOnToggleMethodName );
        EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}