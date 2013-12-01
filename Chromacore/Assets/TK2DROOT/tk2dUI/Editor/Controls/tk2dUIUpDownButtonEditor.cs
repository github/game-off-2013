using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIUpDownButton))]
public class tk2dUIUpDownButtonEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        tk2dUIUpDownButton upDownButton = (tk2dUIUpDownButton)target;

        upDownButton.upStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Up State GameObject", upDownButton.upStateGO,target);
        upDownButton.downStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Down State GameObject", upDownButton.downStateGO,target);

        EditorGUIUtility.LookLikeControls(200);

        bool newUseOnReleaseInsteadOfOnUp = EditorGUILayout.Toggle("Use OnRelease Instead of OnUp", upDownButton.UseOnReleaseInsteadOfOnUp);
        if (newUseOnReleaseInsteadOfOnUp != upDownButton.UseOnReleaseInsteadOfOnUp)
        {
            upDownButton.InternalSetUseOnReleaseInsteadOfOnUp(newUseOnReleaseInsteadOfOnUp);
            GUI.changed = true;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(upDownButton);
        }
    }
}