using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIHoverItem))]
public class tk2dUIHoverItemEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tk2dUIHoverItem hoverBtn = (tk2dUIHoverItem)target;

        hoverBtn.overStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Over State GameObject", hoverBtn.overStateGO,target);
        hoverBtn.outStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Out State GameObject", hoverBtn.outStateGO,target);

        BeginMessageGUI();
        methodBindingUtil.MethodBinding( "On Toggle Hover", typeof(tk2dUIHoverItem), hoverBtn.SendMessageTarget, ref hoverBtn.SendMessageOnToggleHoverMethodName );
        EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(hoverBtn);
        }
    }

}
