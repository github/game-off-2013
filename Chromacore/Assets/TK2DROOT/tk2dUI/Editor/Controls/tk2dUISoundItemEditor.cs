using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUISoundItem))]
public class tk2dUISoundItemEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tk2dUISoundItem soundBtn = (tk2dUISoundItem)target;

        soundBtn.downButtonSound = EditorGUILayout.ObjectField("Down Sound",soundBtn.downButtonSound, typeof(AudioClip),false,null) as AudioClip;
        soundBtn.upButtonSound = EditorGUILayout.ObjectField("Up Sound", soundBtn.upButtonSound, typeof(AudioClip), false, null) as AudioClip;
        soundBtn.clickButtonSound = EditorGUILayout.ObjectField("Click Sound", soundBtn.clickButtonSound, typeof(AudioClip), false, null) as AudioClip;
        soundBtn.releaseButtonSound = EditorGUILayout.ObjectField("Release Sound", soundBtn.releaseButtonSound, typeof(AudioClip), false, null) as AudioClip;
        if (GUI.changed)
        {
            EditorUtility.SetDirty(soundBtn);
        }
    }

}
