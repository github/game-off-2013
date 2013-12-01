using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(tk2dSpriteCollection))]
public class tk2dSpriteCollectionEditor : Editor
{
	const string defaultSpriteCollectionName = "SpriteCollection";
	static bool showDefaultInspector = false;
	static bool viewData = false;

    void OnEnable() {
    	viewData = false;
    }

    public override void OnInspectorGUI()
    {
        tk2dSpriteCollection gen = (tk2dSpriteCollection)target;
		GUILayout.BeginVertical();
		GUILayout.Space(8);

		if (gen.managedSpriteCollection)
		{
			string label = showDefaultInspector?"Hide Default Inspector":"Show Default Inspector";
			int buttonPressed = tk2dGuiUtility.InfoBoxWithButtons("This is a managed sprite collection. Please do not modify.", 
																  tk2dGuiUtility.WarningLevel.Info, 
																  new string[] { label } );
			if (buttonPressed == 0) showDefaultInspector = !showDefaultInspector;
			if (showDefaultInspector) 
			{
				GUILayout.Space(16);
				DrawDefaultInspector();
			}
		}
		else
		{
			string assetPath = AssetDatabase.GetAssetPath(gen).ToLower();
			bool inResources = assetPath.IndexOf("/resources/") != -1;
			
			if (inResources)
			{
				string msg = "Sprite collection is in a resources directory. " +
					"All source textures will be included in build.\n\n" +
					"Editor is disabled. Move it out of the resources directory to continue.";
				tk2dGuiUtility.InfoBox(msg, tk2dGuiUtility.WarningLevel.Error);
			}
			else 
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Open Editor...", GUILayout.MinWidth(120)))
				{
					if (gen.name == defaultSpriteCollectionName)
					{
						EditorUtility.DisplayDialog("Invalid Sprite Collection name", "Please rename sprite collection before proceeding", "Ok");
					}
					else
					{
						tk2dSpriteCollectionEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteCollectionEditorPopup), false, "SpriteCollection" ) as tk2dSpriteCollectionEditorPopup;
						v.SetGenerator(gen);
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}

        EditorGUILayout.EndVertical();

		GUILayout.Space(64);

    	if (viewData && !(gen.managedSpriteCollection && showDefaultInspector)) {
    		DrawDefaultInspector();
    		return;
    	}
    }

    public static tk2dSpriteCollection CreateSpriteCollection(string basePath, string name)
    {
    	string path = AssetDatabase.GenerateUniqueAssetPath( basePath + "/" + name + ".prefab" );
        GameObject go = new GameObject();
        tk2dSpriteCollection spriteCollection = go.AddComponent<tk2dSpriteCollection>();
        spriteCollection.version = tk2dSpriteCollection.CURRENT_VERSION;
        tk2dEditorUtility.SetGameObjectActive(go, false);

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		Object p = EditorUtility.CreateEmptyPrefab(path);
        EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#else
		Object p = PrefabUtility.CreateEmptyPrefab(path);
        PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#endif
		
        GameObject.DestroyImmediate(go);

        return AssetDatabase.LoadAssetAtPath(path, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
	}

    [MenuItem("CONTEXT/tk2dSpriteCollection/View data")]
    static void ToggleViewData() {
        tk2dSpriteCollectionEditor.viewData = true;
    }

	// Menu entries
	[MenuItem("Assets/Create/tk2d/Sprite Collection", false, 10000)]
    static void DoCollectionCreate()
    {
		string path = tk2dEditorUtility.CreateNewPrefab(defaultSpriteCollectionName);
        if (path.Length != 0)
        {
            GameObject go = new GameObject();
            tk2dSpriteCollection spriteCollection = go.AddComponent<tk2dSpriteCollection>();
            spriteCollection.version = tk2dSpriteCollection.CURRENT_VERSION;
            if (tk2dCamera.Editor__Inst != null) {
            	spriteCollection.sizeDef.CopyFrom( tk2dSpriteCollectionSize.ForTk2dCamera( tk2dCamera.Editor__Inst ) );
            }
	        tk2dEditorUtility.SetGameObjectActive(go, false);

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			Object p = EditorUtility.CreateEmptyPrefab(path);
            EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#else
			Object p = PrefabUtility.CreateEmptyPrefab(path);
            PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#endif
			
            GameObject.DestroyImmediate(go);

			// Select object
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }
    }	
}
