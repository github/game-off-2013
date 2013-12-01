using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dPreferences {

	// Serialized fields
	public bool displayTextureThumbs = true;
	
	public bool autoRebuild = true;
	public bool showIds = false;
	public string platform = "";

	public bool enableSpriteHandles = true;
	public bool enableMoveHandles = true;

	public Color tileMapToolColor_brush = new Color32(0, 128, 255, 32);
	public Color tileMapToolColor_brushRandom = new Color32(128, 255, 0, 32);
	public Color tileMapToolColor_erase = new Color32(255, 64, 64, 32);
	public Color tileMapToolColor_eyedropper = new Color32(255, 212, 32, 32);
	public Color tileMapToolColor_cut = new Color32(255, 128, 32, 32);

	public int spriteCollectionListWidth {
		get { return _spriteCollectionListWidth; }
		set { if (_spriteCollectionListWidth != value) _spriteCollectionListWidth = value; Save(); }
	}
	public int spriteCollectionInspectorWidth {
		get { return Mathf.Max(_spriteCollectionInspectorWidth, _minSpriteCollectionInspectorWidth); }
		set { if (_spriteCollectionInspectorWidth != value) _spriteCollectionInspectorWidth = value; Save(); }
	}
	public int animListWidth {
		get { return _animListWidth; }
		set { if (_animListWidth != value) _animListWidth = value; Save(); }
	}
	public int animInspectorWidth {
		get { return Mathf.Max(_animInspectorWidth, _minSpriteCollectionInspectorWidth); }
		set { if (_animInspectorWidth != value) _animInspectorWidth = value; Save(); }
	}
	public int animFrameWidth {
		get { return _animFrameWidth; }
		set { if (_animFrameWidth != value) _animFrameWidth = value; Save(); }
	}
	public int spriteThumbnailSize {
		get { return _spriteThumbnailSize; }
		set { if (_spriteThumbnailSize != value) _spriteThumbnailSize = value; Save(); }
	}

	int _spriteCollectionListWidth = 200;
	int _spriteCollectionInspectorWidth = 260;
	const int _minSpriteCollectionInspectorWidth = 190;
	int _animListWidth = 200;
	int _animInspectorWidth = 260;
	int _animFrameWidth = -1;
	int _spriteThumbnailSize = 128;

	// Grid settings
	[System.Xml.Serialization.XmlIgnoreAttribute]
	public tk2dGrid.Type gridType {
		get { return _gridType; }
		set {
			if (_gridType != value) {
				_gridType = value;
				tk2dGrid.Done();
				Save();
			}
		}
	}
	[System.Xml.Serialization.XmlIgnoreAttribute]
	public Color customGridColor0 {
		get { return _customGridColor0; }
		set { 
			if (_customGridColor0 != value) {
				_customGridColor0 = value;
				tk2dGrid.Done();
				Save();
			}
		}
	}
	[System.Xml.Serialization.XmlIgnoreAttribute]
	public Color customGridColor1 {
		get { return _customGridColor1; }
		set { 
			if (_customGridColor1 != value) {
				_customGridColor1 = value;
				tk2dGrid.Done();
				Save();
			}
		}
	}
	
	public tk2dGrid.Type _gridType = tk2dGrid.Type.DarkChecked;
	public Color _customGridColor0 = Color.white;
	public Color _customGridColor1 = Color.gray;

	// Instance
	static tk2dPreferences _inst = null;
	public static tk2dPreferences inst {
		get {
			if (_inst == null) {
				_inst = Load();
			}
			return _inst;
		}
	}

	public static tk2dPreferences Defaults {
		get { return new tk2dPreferences(); }
	}

	private tk2dPreferences() {
	}

	static tk2dPreferences Load() {
		tk2dPreferences returnValue = null;

		string keyName = "tk2dPreferences";
		if (EditorPrefs.HasKey(keyName)) {
			try {
				string s = EditorPrefs.GetString(keyName, "");
				if (s.Length > 0) {
						System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(s));
					System.Xml.XmlReader reader = new System.Xml.XmlTextReader(ms);
					System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(tk2dPreferences));
					returnValue = (tk2dPreferences)x.Deserialize(reader);
				}
			} 
			catch { }
		}

		if (returnValue == null) {
			returnValue = new tk2dPreferences();
		}

		return returnValue;
	}

	/// <summary>
	/// Save tk2dPreferences
	/// </summary>
	public void Save() {
		System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(tk2dPreferences));
		string xml = "";
		using (var ms = new System.IO.MemoryStream()) {
			x.Serialize( ms, this );
			xml = System.Text.Encoding.UTF8.GetString(ms.ToArray());
		}
		EditorPrefs.SetString("tk2dPreferences", xml);
	}

	public void Print() {
		System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(tk2dPreferences));
		string xml = "";
		using (var ms = new System.IO.MemoryStream()) {
			x.Serialize( ms, this );
			xml = System.Text.Encoding.UTF8.GetString(ms.ToArray());
		}
		Debug.Log(xml);
	}
}

public class tk2dPreferencesEditor : EditorWindow
{
	GUIContent label_spriteThumbnails = new GUIContent("Sprite Thumbnails", "Turn off sprite thumbnails to save memory.");

	GUIContent label_autoRebuild = new GUIContent("Auto Rebuild", "Auto rebuild sprite collections when source textures have changed.");
	GUIContent label_showIds = new GUIContent("Show Ids", "Show sprite and animation Ids.");

	GUIContent label_enableSpriteHandles = new GUIContent("Enable Sprite Controls", "Enable controls for sprite resizing, rotation etc.");
	GUIContent label_enableMoveHandles = new GUIContent("Drag sprite to move", "Allow dragging sprite in all modes. When turned off, this is only available when the Unity move/rotate/scale is not visible.");
	
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
	GUIContent label_proSkin = new GUIContent("Pro Skin", "Select this to use the Dark skin.");
#endif	

	Vector2 scroll = Vector2.zero;

	void OnDestroy() {
		tk2dEditorSkin.Done();
	}

	void OnGUI()
	{
		tk2dPreferences prefs = tk2dPreferences.inst;
		scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		EditorGUIUtility.LookLikeControls(150.0f);
		
		prefs.displayTextureThumbs = EditorGUILayout.Toggle(label_spriteThumbnails, prefs.displayTextureThumbs);

		prefs.autoRebuild = EditorGUILayout.Toggle(label_autoRebuild, prefs.autoRebuild);
		
		prefs.showIds = EditorGUILayout.Toggle(label_showIds, prefs.showIds);

		prefs.gridType = (tk2dGrid.Type)EditorGUILayout.EnumPopup("Grid Type", prefs.gridType);
		if (prefs.gridType == tk2dGrid.Type.Custom) {
			EditorGUI.indentLevel++;
			prefs.customGridColor0 = EditorGUILayout.ColorField("Color 0", prefs.customGridColor0);
			prefs.customGridColor1 = EditorGUILayout.ColorField("Color 1", prefs.customGridColor1);
			EditorGUI.indentLevel--;
		}

		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(" ");
		Rect r = GUILayoutUtility.GetRect(64, 64);
		GUILayout.FlexibleSpace();
		tk2dGrid.Draw(r);
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Reset Editor Sizes"))
		{
			prefs.spriteCollectionListWidth = tk2dPreferences.Defaults.spriteCollectionListWidth;
			prefs.spriteCollectionInspectorWidth = tk2dPreferences.Defaults.spriteCollectionInspectorWidth;
			prefs.animListWidth = tk2dPreferences.Defaults.animListWidth;
			prefs.animInspectorWidth = tk2dPreferences.Defaults.animInspectorWidth;
			prefs.animFrameWidth = tk2dPreferences.Defaults.animFrameWidth;
			GUI.changed = true;
		}

		if (tk2dSystem.inst_NoCreate != null)
		{
			string newPlatform = tk2dGuiUtility.PlatformPopup(tk2dSystem.inst_NoCreate, "Platform", prefs.platform);
			if (newPlatform != prefs.platform)
			{
				prefs.platform = newPlatform;
				UnityEditor.EditorPrefs.SetString("tk2d_platform", newPlatform);
				tk2dSystem.CurrentPlatform = prefs.platform; // mirror to where it matters
				tk2dSystemUtility.PlatformChanged(); // tell the editor things have changed
				tk2dEditorUtility.UnloadUnusedAssets();
			}
		}

		prefs.enableSpriteHandles = EditorGUILayout.Toggle (label_enableSpriteHandles, prefs.enableSpriteHandles);
		bool oldGuiEnable = GUI.enabled;
		GUI.enabled = prefs.enableSpriteHandles;
		EditorGUI.indentLevel++;
		prefs.enableMoveHandles = EditorGUILayout.Toggle( label_enableMoveHandles, prefs.enableMoveHandles );
		EditorGUI.indentLevel--;
		GUI.enabled = oldGuiEnable;

		EditorGUILayout.LabelField("Tilemap Paint Mode Colors");
		++EditorGUI.indentLevel;
		EditorGUI.BeginChangeCheck();
		prefs.tileMapToolColor_brush = EditorGUILayout.ColorField("Brush", prefs.tileMapToolColor_brush);
		prefs.tileMapToolColor_brushRandom = EditorGUILayout.ColorField("Random", prefs.tileMapToolColor_brushRandom);
		prefs.tileMapToolColor_erase = EditorGUILayout.ColorField("Erase", prefs.tileMapToolColor_erase);
		prefs.tileMapToolColor_eyedropper = EditorGUILayout.ColorField("Eyedropper", prefs.tileMapToolColor_eyedropper);
		prefs.tileMapToolColor_cut = EditorGUILayout.ColorField("Cut", prefs.tileMapToolColor_cut);
		if (EditorGUI.EndChangeCheck())
			updateTilemapCursorColor = true;
		--EditorGUI.indentLevel;

		GUILayout.EndScrollView();

		if (GUI.changed) {
			tk2dPreferences.inst.Save();
		}
	}

	static bool updateTilemapCursorColor = false;
	static public bool CheckTilemapCursorColorUpdate() {
		bool result = updateTilemapCursorColor;
		updateTilemapCursorColor = false;
		return result;
	}
}
