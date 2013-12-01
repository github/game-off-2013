using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dSpriteAnimation))]
class tk2dSpriteAnimationEditor : Editor
{
    public static bool viewData = false;

    void OnEnable() {
        viewData = false;
    }

    public override void OnInspectorGUI()
    {
        tk2dSpriteAnimation anim = (tk2dSpriteAnimation)target;
        
        GUILayout.Space(8);
        if (anim != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Editor...", GUILayout.MinWidth(120)))
            {
                tk2dSpriteAnimationEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteAnimationEditorPopup), false, "SpriteAnimation" ) as tk2dSpriteAnimationEditorPopup;
                v.SetSpriteAnimation(anim);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (viewData) {
            GUILayout.BeginVertical("box");
            DrawDefaultInspector();
            GUILayout.EndVertical();
        }

        GUILayout.Space(64);
	}
	
    [MenuItem("CONTEXT/tk2dSpriteAnimation/View data")]
    static void ToggleViewData() {
        tk2dSpriteAnimationEditor.viewData = true;
    }

	[MenuItem("Assets/Create/tk2d/Sprite Animation", false, 10001)]
    static void DoAnimationCreate()
    {
		string path = tk2dEditorUtility.CreateNewPrefab("SpriteAnimation");
        if (path.Length != 0)
        {
            GameObject go = new GameObject();
            go.AddComponent<tk2dSpriteAnimation>();
	        tk2dEditorUtility.SetGameObjectActive(go, false);

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			Object p = EditorUtility.CreateEmptyPrefab(path);
            EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#else
			Object p = PrefabUtility.CreateEmptyPrefab(path);
            PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#endif
            GameObject.DestroyImmediate(go);
			
			tk2dEditorUtility.GetOrCreateIndex().AddSpriteAnimation(AssetDatabase.LoadAssetAtPath(path, typeof(tk2dSpriteAnimation)) as tk2dSpriteAnimation);
			tk2dEditorUtility.CommitIndex();

			// Select object
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }
    }	
}

