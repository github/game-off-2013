using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIToggleButtonGroup))]
public class tk2dUIToggleButtonGroupEditor : Editor
{
    private bool listVisibility = true;
    private SerializedObject serializedObj;

    public void OnEnable()
    {
        serializedObj = new SerializedObject(target);
    }

    public override void  OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        tk2dUIToggleButtonGroup toggleBtnGroup = (tk2dUIToggleButtonGroup)target;

        serializedObj.Update();
        ListIterator("toggleBtns", ref listVisibility);
        serializedObj.ApplyModifiedProperties();

        toggleBtnGroup.SelectedIndex = EditorGUILayout.IntField("Selected Index", toggleBtnGroup.SelectedIndex);

        tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();
        toggleBtnGroup.sendMessageTarget = methodBindingUtil.BeginMessageGUI(toggleBtnGroup.sendMessageTarget);
        methodBindingUtil.MethodBinding( "On Change", typeof(tk2dUIToggleButtonGroup), toggleBtnGroup.sendMessageTarget, ref toggleBtnGroup.SendMessageOnChangeMethodName );
        methodBindingUtil.EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(toggleBtnGroup);
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