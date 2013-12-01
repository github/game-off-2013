using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tk2dFont))]
public class tk2dFontEditor : Editor 
{
	public Shader GetShader(bool gradient)
	{
		if (gradient) return Shader.Find("tk2d/Blend2TexVertexColor");
		else return Shader.Find("tk2d/BlendVertexColor");
	}
	
	public override void OnInspectorGUI()
	{
		tk2dFont gen = (tk2dFont)target;
		if (gen.proxyFont)
		{
			GUILayout.Label("This font is managed by a Sprite Collection");
			return;
		}
		gen.Upgrade();

		EditorGUILayout.BeginVertical();

		DrawDefaultInspector();		
		tk2dGuiUtility.SpriteCollectionSize( gen.sizeDef );
		
		// Warning when texture is compressed
		if (gen.texture != null)
		{
			Texture2D tex = (Texture2D)gen.texture;
			if (tex && IsTextureCompressed(tex))
			{
				int buttonPressed;
				if ((buttonPressed = tk2dGuiUtility.InfoBoxWithButtons(
					"Font texture appears to be compressed. " +
					"Quality will be lost and the texture may appear blocky in game.\n" +
					"Do you wish to change the format?", 
					tk2dGuiUtility.WarningLevel.Warning, 
					new string[] { "16bit", "Truecolor" }
					)) != -1)
				{
					if (buttonPressed == 0)
					{
						ConvertTextureToFormat(tex, TextureImporterFormat.Automatic16bit);
					}
					else
					{
						ConvertTextureToFormat(tex, TextureImporterFormat.AutomaticTruecolor);
					}
				}
			}
		}
		
		// Warning when gradient texture is compressed
		if (gen.gradientTexture != null && 
			(gen.gradientTexture.format != TextureFormat.ARGB32 && gen.gradientTexture.format != TextureFormat.RGB24 && gen.gradientTexture.format != TextureFormat.RGBA32))
		{
			if (tk2dGuiUtility.InfoBoxWithButtons(
				"The gradient texture should be truecolor for best quality. " +
				"Current format is " + gen.gradientTexture.format.ToString() + ".",
				tk2dGuiUtility.WarningLevel.Warning,
				new string[] { "Fix" }
				) != -1)
			{
				ConvertTextureToFormat(gen.gradientTexture, TextureImporterFormat.AutomaticTruecolor);
			}
		}

		if (GUILayout.Button("Commit..."))
		{
			if (gen.bmFont == null || gen.texture == null)
			{
				EditorUtility.DisplayDialog("BMFont", "Need an bmFont and texture bound to work", "Ok");
				return;
			}
			
			if (gen.material == null)
			{
				gen.material = new Material(GetShader(gen.gradientTexture != null));
				string materialPath = AssetDatabase.GetAssetPath(gen).Replace(".prefab", "material.mat");
				AssetDatabase.CreateAsset(gen.material, materialPath);
			}
			
			if (gen.data == null)
			{
				string bmFontPath = AssetDatabase.GetAssetPath(gen).Replace(".prefab", "data.prefab");
				
				GameObject go = new GameObject();
				go.AddComponent<tk2dFontData>();
				tk2dEditorUtility.SetGameObjectActive(go, false);
				
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
				Object p = EditorUtility.CreateEmptyPrefab(bmFontPath);
				EditorUtility.ReplacePrefab(go, p);
#else
				Object p = PrefabUtility.CreateEmptyPrefab(bmFontPath);
				PrefabUtility.ReplacePrefab(go, p);
#endif
				GameObject.DestroyImmediate(go);
				AssetDatabase.SaveAssets();
				
				gen.data = AssetDatabase.LoadAssetAtPath(bmFontPath, typeof(tk2dFontData)) as tk2dFontData;
			}
			
			ParseBMFont(AssetDatabase.GetAssetPath(gen.bmFont), gen.data, gen);

			if (gen.manageMaterial)
			{
				Shader s = GetShader(gen.gradientTexture != null);
				if (gen.material.shader != s)
				{
					gen.material.shader = s;
					EditorUtility.SetDirty(gen.material);
				}
				if (gen.material.mainTexture != gen.texture)
				{
					gen.material.mainTexture = gen.texture;
					EditorUtility.SetDirty(gen.material);
				}
				if (gen.gradientTexture != null && gen.gradientTexture != gen.material.GetTexture("_GradientTex"))
				{
					gen.material.SetTexture("_GradientTex", gen.gradientTexture);
					EditorUtility.SetDirty(gen.material);
				}
			}
			
			gen.data.version = tk2dFontData.CURRENT_VERSION;

			gen.data.material = gen.material;
			gen.data.textureGradients = gen.gradientTexture != null;
			gen.data.gradientCount = gen.gradientCount;
			gen.data.gradientTexture = gen.gradientTexture;
			
			gen.data.invOrthoSize = 1.0f / gen.sizeDef.OrthoSize;
			gen.data.halfTargetHeight = gen.sizeDef.TargetHeight * 0.5f;
			
            // Rebuild assets already present in the scene
            tk2dTextMesh[] sprs = Resources.FindObjectsOfTypeAll(typeof(tk2dTextMesh)) as tk2dTextMesh[];
            foreach (tk2dTextMesh spr in sprs)
            {
                spr.Init(true);
            }
			
			EditorUtility.SetDirty(gen);
			EditorUtility.SetDirty(gen.data);

			// update index
			tk2dEditorUtility.GetOrCreateIndex().AddOrUpdateFont(gen);
			tk2dEditorUtility.CommitIndex();
        }

		EditorGUILayout.EndVertical();

		GUILayout.Space(64);
	}
	
	bool IsTextureCompressed(Texture2D texture)
	{
		if (texture.format == TextureFormat.ARGB32 
			|| texture.format == TextureFormat.ARGB4444 
			|| texture.format == TextureFormat.Alpha8 
			|| texture.format == TextureFormat.RGB24 
			|| texture.format == TextureFormat.RGB565 
			|| texture.format == TextureFormat.RGBA32)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	
	void ConvertTextureToFormat(Texture2D texture, TextureImporterFormat format)
	{
		string assetPath = AssetDatabase.GetAssetPath(texture);
		if (assetPath != "")
		{
			// make sure the source texture is npot and readable, and uncompressed
        	TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
			if (importer.textureFormat != format)
				importer.textureFormat = format;
			
			AssetDatabase.ImportAsset(assetPath);
		}
	}
	
	
	
	
	bool ParseBMFont(string path, tk2dFontData fontData, tk2dFont source)
	{
		float scale = 2.0f * source.sizeDef.OrthoSize / source.sizeDef.TargetHeight;
		
		tk2dEditor.Font.Info fontInfo = tk2dEditor.Font.Builder.ParseBMFont(path);
		if (fontInfo != null)
			return tk2dEditor.Font.Builder.BuildFont(fontInfo, fontData, scale, source.charPadX, source.dupeCaps, source.flipTextureY, source.gradientTexture, source.gradientCount);
		else
			return false;
	}

	[MenuItem("Assets/Create/tk2d/Font", false, 11000)]
	static void DoBMFontCreate()
	{
		string path = tk2dEditorUtility.CreateNewPrefab("Font");
		if (path.Length != 0)
		{
			GameObject go = new GameObject();
			tk2dFont font = go.AddComponent<tk2dFont>();
			font.manageMaterial = true;
			font.version = tk2dFont.CURRENT_VERSION;
			if (tk2dCamera.Editor__Inst != null) {
				font.sizeDef.CopyFrom( tk2dSpriteCollectionSize.ForTk2dCamera( tk2dCamera.Editor__Inst ) );
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
