using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUITweenItem))]
public class tk2dUITweenItemEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUIUtility.LookLikeControls(200);
        tk2dUITweenItem btnClickScaler = (tk2dUITweenItem)target;

        btnClickScaler.onDownScale = EditorGUILayout.Vector3Field("On Down Scale", btnClickScaler.onDownScale);
        btnClickScaler.tweenDuration = EditorGUILayout.FloatField("Tween Duration", btnClickScaler.tweenDuration);
        btnClickScaler.canButtonBeHeldDown = EditorGUILayout.Toggle("Can Button Be Held Down?", btnClickScaler.canButtonBeHeldDown);

        bool newUseOnReleaseInsteadOfOnUp = EditorGUILayout.Toggle("Use OnRelease Instead of OnUp", btnClickScaler.UseOnReleaseInsteadOfOnUp);
        if (newUseOnReleaseInsteadOfOnUp != btnClickScaler.UseOnReleaseInsteadOfOnUp)
        {
            btnClickScaler.InternalSetUseOnReleaseInsteadOfOnUp(newUseOnReleaseInsteadOfOnUp);
            GUI.changed = true;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(btnClickScaler);
        }
    }
}
