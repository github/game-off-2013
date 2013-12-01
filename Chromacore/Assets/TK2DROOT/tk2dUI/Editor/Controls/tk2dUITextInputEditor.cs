using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUITextInput))]
public class tk2dUITextInputEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        base.OnInspectorGUI();

		tk2dUITextInput textInput = (tk2dUITextInput)target;
		textInput.LayoutItem = EditorGUILayout.ObjectField("LayoutItem", textInput.LayoutItem, typeof(tk2dUILayout), true) as tk2dUILayout;

        tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();
        textInput.SendMessageTarget = methodBindingUtil.BeginMessageGUI(textInput.SendMessageTarget);
        methodBindingUtil.MethodBinding( "On Text Change", typeof(tk2dUITextInput), textInput.SendMessageTarget, ref textInput.SendMessageOnTextChangeMethodName );
        methodBindingUtil.EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(textInput);
        }
    }

    public void OnSceneGUI()
    {
        bool wasChange=false;
        tk2dUITextInput textInput = (tk2dUITextInput)target;

        // Get rescaled transforms
        Matrix4x4 m = textInput.inputLabel.transform.localToWorldMatrix;
        Vector3 right = m.MultiplyVector(Vector3.right);

        float newFieldLength = tk2dUIControlsHelperEditor.DrawLengthHandles("Field Length", textInput.fieldLength, textInput.inputLabel.transform.position, right, Color.red, -.1f, .25f, 0);
        if (newFieldLength != textInput.fieldLength)
        {
            Undo.RegisterUndo(textInput, "Field length changed");
            textInput.fieldLength = newFieldLength;
            wasChange = true;
        }

        if (wasChange)
        {
            EditorUtility.SetDirty(textInput);
        }
    }

}
