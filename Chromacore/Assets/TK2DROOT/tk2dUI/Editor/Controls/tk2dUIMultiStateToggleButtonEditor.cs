using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIMultiStateToggleButton))]
public class tk2dUIMultiStateToggleButtonEditor : tk2dUIBaseItemControlEditor
{
    private bool listVisibility = true;
    private SerializedObject serializedObj;

    public void OnEnable()
    {
        serializedObj = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tk2dUIMultiStateToggleButton multiStateToggleBtn = (tk2dUIMultiStateToggleButton)target;

        serializedObj.Update();
        ListIterator("states", ref listVisibility);
        serializedObj.ApplyModifiedProperties();

        multiStateToggleBtn.activateOnPress = EditorGUILayout.Toggle("Activate On Press", multiStateToggleBtn.activateOnPress);

        BeginMessageGUI();
        methodBindingUtil.MethodBinding( "On State Toggle", typeof(tk2dUIMultiStateToggleButton), multiStateToggleBtn.SendMessageTarget, ref multiStateToggleBtn.SendMessageOnStateToggleMethodName );
        EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(multiStateToggleBtn);
        }
    }

    //http://answers.unity3d.com/questions/200123/editor-how-to-do-propertyfield-for-list-elements.html?sort=oldest
    public void ListIterator(string propertyPath, ref bool visible)
    {
        SerializedProperty listProperty = serializedObj.FindProperty(propertyPath);
        visible = EditorGUILayout.Foldout(visible, ObjectNames.NicifyVariableName(listProperty.name));

        if (visible)
        {
            EditorGUI.indentLevel++;
            string newArraySizeStr = EditorGUILayout.TextField("Size:", "" + listProperty.arraySize);
            int newArraySize = listProperty.arraySize;
            if (!int.TryParse(newArraySizeStr, out newArraySize))
            {
                newArraySize = listProperty.arraySize;
            }

            if (newArraySize != listProperty.arraySize)
            {
                serializedObj.FindProperty(propertyPath + ".Array.size").intValue = newArraySize;
            }

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                Rect drawZone = GUILayoutUtility.GetRect(0f, 16f);
                /*bool showChildren = */
                EditorGUI.PropertyField(drawZone, elementProperty);
            }
            EditorGUI.indentLevel--;
        }
    }

}