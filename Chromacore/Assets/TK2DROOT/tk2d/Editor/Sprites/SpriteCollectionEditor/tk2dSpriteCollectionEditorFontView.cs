using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	public class FontView
	{
		public SpriteCollectionProxy SpriteCollection { get { return host.SpriteCollection; } }
		
		IEditorHost host;
		public FontView(IEditorHost host)
		{
			this.host = host;
		}
	
		Vector2 fontTextureScrollBar;
		Vector2 fontEditorScrollBar;
		public bool Draw(List<SpriteCollectionEditorEntry> selectedEntries)
		{
			if (selectedEntries.Count == 0 || selectedEntries[0].type != SpriteCollectionEditorEntry.Type.Font)
				return false;
			
			var entry = selectedEntries[selectedEntries.Count - 1];
			var font = SpriteCollection.fonts[ entry.index ];
			
			bool doDelete = false;
			GUILayout.BeginHorizontal();
			
			// Body
			GUILayout.BeginVertical(tk2dEditorSkin.SC_BodyBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			fontTextureScrollBar = GUILayout.BeginScrollView(fontTextureScrollBar);
			if (font.texture != null)
			{
				font.texture.filterMode = FilterMode.Point;
				int border = 16;
				Rect rect = GUILayoutUtility.GetRect(border + font.texture.width, border + font.texture.height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				tk2dGrid.Draw(rect);
				GUI.Label(new Rect(border + rect.x, border + rect.y, font.texture.width, font.texture.height), font.texture);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
			// Inspector
			EditorGUIUtility.LookLikeControls(100.0f, 100.0f);
			fontEditorScrollBar = GUILayout.BeginScrollView(fontEditorScrollBar, GUILayout.ExpandHeight(true), GUILayout.Width(host.InspectorWidth));
			
			// Header
			GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorHeaderBG, GUILayout.ExpandWidth(true));
			Object newBmFont = EditorGUILayout.ObjectField("BM Font", font.bmFont, typeof(Object), false);
			if (newBmFont != font.bmFont)
			{
				font.texture = null;
				entry.name = "Empty";
				font.bmFont = newBmFont;
				if (newBmFont != null)
				{
					string bmFontPath = AssetDatabase.GetAssetPath(newBmFont);
					tk2dEditor.Font.Info fontInfo = tk2dEditor.Font.Builder.ParseBMFont(bmFontPath);
					if (fontInfo != null && fontInfo.texturePaths.Length > 0)
					{
						string path = System.IO.Path.GetDirectoryName(bmFontPath).Replace('\\', '/') + "/" + System.IO.Path.GetFileName(fontInfo.texturePaths[0]);;
						font.texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
					}
	
					entry.name = font.Name;
					host.OnSpriteCollectionSortChanged();
				}
			}
			GUILayout.BeginHorizontal();
			Texture2D newTexture = EditorGUILayout.ObjectField("Font Texture", font.texture, typeof(Texture2D), false) as Texture2D;
			if (newTexture != font.texture)
			{
				font.texture = newTexture;
				entry.name = font.Name;
				host.OnSpriteCollectionSortChanged();
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete", EditorStyles.miniButton)) doDelete = true;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			
			
			// Rest of inspector
			GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.ExpandWidth(true));

			if (font.texture != null)
			{
				string assetPath = AssetDatabase.GetAssetPath(font.texture);
				if (assetPath.Length > 0)
				{
					// make sure the source texture is npot and readable, and uncompressed
					if (!tk2dSpriteCollectionBuilder.IsTextureImporterSetUp(assetPath))
					{
						if (tk2dGuiUtility.InfoBoxWithButtons(
							"The texture importer needs to be reconfigured to be used as a font texture source. " +
							"Please note that this will globally change this texture importer. ",
							tk2dGuiUtility.WarningLevel.Info,
							"Set up") != -1)
						{
							tk2dSpriteCollectionBuilder.ConfigureSpriteTextureImporter(assetPath);
							AssetDatabase.ImportAsset(assetPath);
						}						
					}
				}			
			}

			if (SpriteCollection.AllowAltMaterials && SpriteCollection.altMaterials.Length > 1)
			{
				List<int> altMaterialIndices = new List<int>();
				List<string> altMaterialNames = new List<string>();
				for (int i = 0; i < SpriteCollection.altMaterials.Length; ++i)
				{
					var mat = SpriteCollection.altMaterials[i];
					if (mat == null) continue;
					altMaterialIndices.Add(i);
					altMaterialNames.Add(mat.name);
				}
				font.materialId = EditorGUILayout.IntPopup("Material", font.materialId, altMaterialNames.ToArray(), altMaterialIndices.ToArray());
			}

			if (font.data == null || font.editorData == null)
			{
				if (tk2dGuiUtility.InfoBoxWithButtons(
					"A data object is required to build a font. " +
					"Please create one or drag an existing data object into the inspector slot.\n",
					tk2dGuiUtility.WarningLevel.Info, 
					"Create") != -1)
				{
					// make data folder
					string root = SpriteCollection.GetOrCreateDataPath();

					string name = font.bmFont?font.bmFont.name:"Unknown Font";
					string editorDataPath = tk2dGuiUtility.SaveFileInProject("Save Font Data", root, name, "prefab");
					if (editorDataPath.Length > 0)
					{
						int prefabOffset = editorDataPath.ToLower().IndexOf(".prefab");
						string dataObjectPath = editorDataPath.Substring(0, prefabOffset) + " data.prefab";
						
						// Create data object
						{
							GameObject go = new GameObject();
							go.AddComponent<tk2dFontData>();
					        tk2dEditorUtility.SetGameObjectActive(go, false);
	#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
							Object p = EditorUtility.CreateEmptyPrefab(dataObjectPath);
							EditorUtility.ReplacePrefab(go, p);
	#else
							Object p = PrefabUtility.CreateEmptyPrefab(dataObjectPath);
							PrefabUtility.ReplacePrefab(go, p);
	#endif
							GameObject.DestroyImmediate(go);
							AssetDatabase.SaveAssets();
							font.data = AssetDatabase.LoadAssetAtPath(dataObjectPath, typeof(tk2dFontData)) as tk2dFontData;
						}
						
						// Create editor object
						{
							GameObject go = new GameObject();
							tk2dFont f = go.AddComponent<tk2dFont>();
							f.proxyFont = true;
							f.data = font.data;
					        tk2dEditorUtility.SetGameObjectActive(go, false);
				
				#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
							Object p = EditorUtility.CreateEmptyPrefab(editorDataPath);
							EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
				#else
							Object p = PrefabUtility.CreateEmptyPrefab(editorDataPath);
							PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
				#endif
							GameObject.DestroyImmediate(go);
							
							tk2dFont loadedFont = AssetDatabase.LoadAssetAtPath(editorDataPath, typeof(tk2dFont)) as tk2dFont;
							tk2dEditorUtility.GetOrCreateIndex().AddOrUpdateFont(loadedFont);
							tk2dEditorUtility.CommitIndex();
							
							font.editorData = AssetDatabase.LoadAssetAtPath(editorDataPath, typeof(tk2dFont)) as tk2dFont;
						}
						
						entry.name = font.Name;
						host.OnSpriteCollectionSortChanged();
					}
				}
			}
			else
			{
				font.editorData = EditorGUILayout.ObjectField("Editor Data", font.editorData, typeof(tk2dFont), false) as tk2dFont;
				font.data = EditorGUILayout.ObjectField("Font Data", font.data, typeof(tk2dFontData), false) as tk2dFontData;
			}

			if (font.data && font.editorData)
			{
				font.useGradient = EditorGUILayout.Toggle("Use Gradient", font.useGradient);
				if (font.useGradient)
				{
					EditorGUI.indentLevel++;
					Texture2D tex = EditorGUILayout.ObjectField("Gradient Tex", font.gradientTexture, typeof(Texture2D), false) as Texture2D;
					if (font.gradientTexture != tex)
					{
						font.gradientTexture = tex;

						List<Material> materials = new List<Material>();
						materials.Add( SpriteCollection.altMaterials[font.materialId] );

						for (int j = 0; j < SpriteCollection.platforms.Count; ++j)
						{
							if (!SpriteCollection.platforms[j].Valid) continue;
							tk2dSpriteCollection data = SpriteCollection.platforms[j].spriteCollection;
							materials.Add( data.altMaterials[font.materialId] );
						}

						for (int j = 0; j < materials.Count; ++j)
						{
							if (!materials[j].HasProperty("_GradientTex"))
							{
								Debug.LogError(string.Format("Cant find parameter '_GradientTex' in material '{0}'", materials[j].name));
							}
							else if (materials[j].GetTexture("_GradientTex") != tex)
							{
								materials[j].SetTexture("_GradientTex", font.gradientTexture);
								EditorUtility.SetDirty(materials[j]);
							}
						}
					}
	
					font.gradientCount = EditorGUILayout.IntField("Gradient Count", font.gradientCount);
					EditorGUI.indentLevel--;
				}
			}
			
			//font.dupeCaps = EditorGUILayout.Toggle("Dupe caps", font.dupeCaps);
			font.flipTextureY = EditorGUILayout.Toggle("Flip Texture Y", font.flipTextureY);
			font.charPadX = EditorGUILayout.IntField("Char Pad X", font.charPadX);
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

			// make dragable
			tk2dPreferences.inst.spriteCollectionInspectorWidth -= (int)tk2dGuiUtility.DragableHandle(4819284, GUILayoutUtility.GetLastRect(), 0, tk2dGuiUtility.DragDirection.Horizontal);
			
			GUILayout.EndHorizontal();
			
			if (doDelete &&
				EditorUtility.DisplayDialog("Delete sprite", "Are you sure you want to delete the selected font?", "Yes", "No"))
			{
				font.active = false;
				font.bmFont = null;
				font.data = null;
				font.texture = null;
				SpriteCollection.Trim();
				host.OnSpriteCollectionChanged(false);
			}
			
			return true;
		}
	}
}
