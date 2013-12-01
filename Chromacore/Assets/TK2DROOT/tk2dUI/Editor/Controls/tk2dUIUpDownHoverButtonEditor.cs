using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIUpDownHoverButton))]
public class tk2dUIUpDownHoverButtonEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        tk2dUIUpDownHoverButton upDownHoverButton = (tk2dUIUpDownHoverButton)target;

        upDownHoverButton.upStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Up State GameObject", upDownHoverButton.upStateGO,target);
        upDownHoverButton.downStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Down State GameObject", upDownHoverButton.downStateGO,target);
        upDownHoverButton.hoverOverStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Hover State GameObject", upDownHoverButton.hoverOverStateGO, target);

        EditorGUIUtility.LookLikeControls(200);

        bool newUseOnReleaseInsteadOfOnUp = EditorGUILayout.Toggle("Use OnRelease Instead of OnUp", upDownHoverButton.UseOnReleaseInsteadOfOnUp);
        if (newUseOnReleaseInsteadOfOnUp != upDownHoverButton.UseOnReleaseInsteadOfOnUp)
        {
            upDownHoverButton.InternalSetUseOnReleaseInsteadOfOnUp(newUseOnReleaseInsteadOfOnUp);
            GUI.changed = true;
        }

        BeginMessageGUI();
        methodBindingUtil.MethodBinding( "On Toggle Over", typeof(tk2dUIUpDownHoverButton), upDownHoverButton.SendMessageTarget, ref upDownHoverButton.SendMessageOnToggleOverMethodName );
        EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(upDownHoverButton);
        }
    }
}