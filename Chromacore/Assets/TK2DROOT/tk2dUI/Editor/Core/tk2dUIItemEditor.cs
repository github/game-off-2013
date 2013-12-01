using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIItem))]
public class tk2dUIItemEditor : Editor
{
    SerializedProperty extraBoundsProp;
    SerializedProperty ignoreBoundsProp;

    void OnEnable() {
        extraBoundsProp = serializedObject.FindProperty("editorExtraBounds");
        ignoreBoundsProp = serializedObject.FindProperty("editorIgnoreBounds");
    }

    tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool changeOccurred = false;
        EditorGUIUtility.LookLikeControls(180);
        tk2dUIItem btn = (tk2dUIItem)target;

        bool newIsChildOfAnotherMenuBtn = EditorGUILayout.Toggle("Child of Another UIItem?", btn.InternalGetIsChildOfAnotherUIItem());

        if (newIsChildOfAnotherMenuBtn != btn.InternalGetIsChildOfAnotherUIItem())
        {
            changeOccurred = true;
            btn.InternalSetIsChildOfAnotherUIItem(newIsChildOfAnotherMenuBtn);
        }

        btn.registerPressFromChildren = EditorGUILayout.Toggle("Register Events From Children", btn.registerPressFromChildren);

        btn.isHoverEnabled = EditorGUILayout.Toggle("Is Hover Events Enabled?", btn.isHoverEnabled);

        btn.sendMessageTarget = methodBindingUtil.BeginMessageGUI(btn.sendMessageTarget);
        methodBindingUtil.MethodBinding( "On Down", typeof(tk2dUIItem), btn.sendMessageTarget, ref btn.SendMessageOnDownMethodName );
        methodBindingUtil.MethodBinding( "On Up", typeof(tk2dUIItem), btn.sendMessageTarget, ref btn.SendMessageOnUpMethodName );
        methodBindingUtil.MethodBinding( "On Click", typeof(tk2dUIItem), btn.sendMessageTarget, ref btn.SendMessageOnClickMethodName );
        methodBindingUtil.MethodBinding( "On Release", typeof(tk2dUIItem), btn.sendMessageTarget, ref btn.SendMessageOnReleaseMethodName );
        methodBindingUtil.EndMessageGUI();

        if (btn.collider != null) {
            GUILayout.Label("Collider", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Automatic Fit");
            if (GUILayout.Button("Fit", GUILayout.MaxWidth(100))) {
                tk2dUIItemBoundsHelper.FixColliderBounds(btn);
            }
            GUILayout.EndHorizontal();

            ArrayProperty("Extra Bounds", extraBoundsProp);
            ArrayProperty("Ignore Bounds", ignoreBoundsProp);

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed || changeOccurred)
        {
            EditorUtility.SetDirty(btn);
        }
    }

    public static void ArrayProperty(string name, SerializedProperty prop) {
        SerializedProperty localProp = prop.Copy();
        EditorGUIUtility.LookLikeInspector();
        if ( EditorGUILayout.PropertyField(localProp, new GUIContent(name)) ) {
            EditorGUI.indentLevel++;
            bool expanded = true;
            int depth = localProp.depth;
            while (localProp.NextVisible( expanded ) && depth < localProp.depth) {
                expanded = EditorGUILayout.PropertyField(localProp);
            }
            EditorGUI.indentLevel--;
        }
    }

    //checks through hierarchy to find UIItem at this level or above to be used in inspector field
    public static tk2dUIItem FindAppropriateButtonInHierarchy(GameObject go)
    {
        tk2dUIItem btn = null;

        while (go != null)
        {
            btn = go.GetComponent<tk2dUIItem>();
            if (btn != null)
            {
                break;
            }

            go = go.transform.parent.gameObject;
        }

        return btn;
    }

    //locates tk2dUIManager in scene
    public static tk2dUIManager FindUIManagerInScene()
    {
        return GameObject.FindObjectOfType(typeof(tk2dUIManager)) as tk2dUIManager;
    }

    //creates tk2dUIManager
    [MenuItem("GameObject/Create Other/tk2d/UI Manager", false, 13950)]
    static void CreateUIManager()
    {
        GameObject go = tk2dEditorUtility.CreateGameObjectInScene("tk2dUIManager");
        go.transform.parent = null;
        go.transform.position = Vector3.zero;
        go.AddComponent<tk2dUIManager>();

        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Create tk2dUIManager");
    }
}