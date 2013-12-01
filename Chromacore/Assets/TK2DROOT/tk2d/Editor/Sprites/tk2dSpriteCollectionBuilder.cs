using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class tk2dSpriteCollectionBuilder
{
	class SpriteLut
	{
		public int source; // index into source texture list, will only have multiple entries with same source, when splitting
		public Texture2D sourceTex;
		public Texture2D tex; // texture to atlas
		
		public bool isSplit; // is this part of a split?
		public int rx, ry, rw, rh; // split rectangle in texture coords
		
		public bool isDuplicate; // is this a duplicate texture?
		public int atlasIndex; // index in the atlas
		public string hash; // hash of the tex data and rect
		
		public bool isFont;
		public int fontId;
		public int charId;
	}
	
	public static void ResetCurrentBuild()
	{
		currentBuild = null;
	}
	
	/// <summary>
	/// Determines whether this texture is used as a sprite in a sprite collection
	/// </summary>
	public static bool IsSpriteSourceTexture(string path)
	{
		// This should only take existing indices, as we don't want to slow anything down here
		tk2dIndex index = tk2dEditorUtility.GetExistingIndex();
		if (index == null)
			return false;

		tk2dSpriteCollectionIndex[] scg = index.GetSpriteCollectionIndex();
		if (scg == null)
			return false;

        foreach (tk2dSpriteCollectionIndex thisScg in scg)
        {
			foreach (var textureGUID in thisScg.spriteTextureGUIDs)
			{
				if (textureGUID == AssetDatabase.AssetPathToGUID(path))
				{
					return true;
				}
			}
		}
		
		return false;
	}
	
	public static bool IsTextureImporterSetUp(string assetPath)
	{
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
        if (importer.textureType != TextureImporterType.Advanced ||
            importer.textureFormat != TextureImporterFormat.AutomaticTruecolor ||
            importer.npotScale != TextureImporterNPOTScale.None ||
            importer.isReadable != true ||
		    importer.maxTextureSize < 4096)
		{
			return false;
		}	
		return true;
	}
	
	public static bool ConfigureSpriteTextureImporter(string assetPath)
	{
		// make sure the source texture is npot and readable, and uncompressed
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
        if (importer.textureType != TextureImporterType.Advanced ||
            importer.textureFormat != TextureImporterFormat.AutomaticTruecolor ||
            importer.npotScale != TextureImporterNPOTScale.None ||
            importer.isReadable != true ||
		    importer.maxTextureSize < 4096)
        {
            importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
            importer.textureType = TextureImporterType.Advanced;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
			importer.mipmapEnabled = false;
			importer.maxTextureSize = 4096;

			return true;
        }
		
		return false;
	}

	// Rebuild a sprite collection when out of date
	// Identifies changed textures by comparing GUID
	public static void RebuildOutOfDate(string[] changedPaths)
    {
		// This should only take existing indices, as we don't want to slow anything down here
		tk2dIndex index = tk2dEditorUtility.GetExistingIndex();
		if (index == null)
			return;

		tk2dSpriteCollectionIndex[] scg = index.GetSpriteCollectionIndex();
		if (scg == null)
			return;

        foreach (tk2dSpriteCollectionIndex thisScg in scg)
        {
			List<string> thisChangedPaths = new List<string>();

			bool checkTimeStamps = false;
			if (thisScg.spriteTextureTimeStamps != null && thisScg.spriteTextureTimeStamps.Length == thisScg.spriteTextureGUIDs.Length)
				checkTimeStamps = true;

	    	for (int i = 0; i < thisScg.spriteTextureGUIDs.Length; ++i)
			{
				string textureGUID = thisScg.spriteTextureGUIDs[i];
				foreach (string changedPath in changedPaths)
				{
					if (textureGUID == AssetDatabase.AssetPathToGUID(changedPath))
					{
						if (checkTimeStamps && System.IO.File.Exists(changedPath))
						{
							string timeStampStr = (System.IO.File.GetLastWriteTime(changedPath) - new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds.ToString();
							if (timeStampStr != thisScg.spriteTextureTimeStamps[i])
								thisChangedPaths.Add(changedPath); // timestamps don't match, file is likely to have changed
						}
						else
						{
							thisChangedPaths.Add(changedPath);
						}
					}
				}
			}

            if (thisChangedPaths.Count > 0)
            {
				tk2dSpriteCollection spriteCollectionSource = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath(thisScg.spriteCollectionGUID), typeof(tk2dSpriteCollection) ) as tk2dSpriteCollection;
				if (spriteCollectionSource != null)
				{
            		Rebuild(spriteCollectionSource);
				}

				spriteCollectionSource = null;
				tk2dEditorUtility.UnloadUnusedAssets();
            }
        }
    }
	
	static int defaultPad = 2;
	
	static int GetPadAmount(tk2dSpriteCollection gen, int spriteId)
	{
		int basePadAmount = 0;
		
		if (gen.padAmount == -1) basePadAmount = (gen.filterMode == FilterMode.Point)?0:defaultPad;
		else basePadAmount = gen.padAmount;
		
		if (spriteId >= 0)
			basePadAmount += (gen.textureParams[spriteId].extraPadding==-1)?0:gen.textureParams[spriteId].extraPadding;
		
		return Mathf.Max(0, basePadAmount);
	}

	static void PadTexture(Texture2D tex, int pad, tk2dSpriteCollectionDefinition.Pad padMode)
	{
		Color bgColor = new Color(0,0,0,0);
		Color c0 = bgColor, c1 = bgColor;
		for (int y = 0; y < pad; ++y)
		{
			for (int x = 0; x < tex.width; ++x)
			{
				switch (padMode) {
					case tk2dSpriteCollectionDefinition.Pad.Extend: c0 = tex.GetPixel(x, pad); c1 = tex.GetPixel(x, tex.height - 1 - pad); break;
					case tk2dSpriteCollectionDefinition.Pad.TileXY: c1 = tex.GetPixel(x, pad); c0 = tex.GetPixel(x, tex.height - 1 - pad); break;
				}
				tex.SetPixel(x, y, c0);
				tex.SetPixel(x, tex.height - 1 - y, c1);
			}
		}
		for (int x = 0; x < pad; ++x)
		{
			for (int y = 0; y < tex.height; ++y)
			{
				switch (padMode) {
					case tk2dSpriteCollectionDefinition.Pad.Extend: c0 = tex.GetPixel(pad, y); c1 = tex.GetPixel(tex.width - 1 - pad, y); break;
					case tk2dSpriteCollectionDefinition.Pad.TileXY: c1 = tex.GetPixel(pad, y); c0 = tex.GetPixel(tex.width - 1 - pad, y); break;
				}
				tex.SetPixel(x, y, c0);
				tex.SetPixel(tex.width - 1 - x, y, c1);
			}
		}
	}



	static void SetUpSourceTextureFormats(tk2dSpriteCollection gen)
	{
		// make sure all textures are in the right format
		int numTexturesReimported = 0;
		List<Texture2D> texturesToProcess = new List<Texture2D>();
		
		for (int i = 0; i < gen.textureParams.Length; ++i)
		{
			if (gen.textureParams[i].texture != null)
			{
				texturesToProcess.Add(gen.textureParams[i].texture);
			}
		}
		if (gen.spriteSheets != null)
		{
			foreach (var v in gen.spriteSheets)
			{
				if (v.texture != null)
					texturesToProcess.Add(v.texture);
			}
		}
		if (gen.fonts != null)
		{
			foreach (var v in gen.fonts)
			{
				if (v.active && v.texture != null)
					texturesToProcess.Add(v.texture);
			}
		}
		foreach (var tex in texturesToProcess)
		{
			string thisTexturePath = AssetDatabase.GetAssetPath(tex);
			if (ConfigureSpriteTextureImporter(thisTexturePath))
			{
				numTexturesReimported++;
	            AssetDatabase.ImportAsset(thisTexturePath);
			}
		}
		if (numTexturesReimported > 0)
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	static void DeleteUnusedAssets<T>( List<T> oldAssets, T[] newAssets ) where T : UnityEngine.Object
	{
		foreach (T asset in oldAssets)
		{
			bool found = false;
			foreach (T t in newAssets)
			{
				if (t == asset)
				{
					found = true;
					break;
				}
			}
			if (!found)
				UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(asset));
		}
	}

	static bool TextureRectFullySolid( Texture2D srcTex, int sx, int sy, int tw, int th ) {
		for (int y = 0; y < th; ++y) {
			for (int x = 0; x < tw; ++x) {
				Color32 col = srcTex.GetPixel( sx + x, sy + y );
				if (col.a < 255) {
					return false;
				}
			}
		}
		return true;
	}

	static Texture2D ProcessTexture(tk2dSpriteCollection settings, bool additive, tk2dSpriteCollectionDefinition.Pad padMode, bool disableTrimming, bool isInjectedTexture, Texture2D srcTex, int sx, int sy, int tw, int th, ref SpriteLut spriteLut, int padAmount)
	{
		// Can't have additive without premultiplied alpha
		if (!settings.premultipliedAlpha) additive = false;
		bool allowTrimming = !settings.disableTrimming && !disableTrimming;
		var textureCompression = settings.textureCompression;
		
		int[] ww = new int[tw];
		int[] hh = new int[th];
		for (int x = 0; x < tw; ++x) ww[x] = 0;
		for (int y = 0; y < th; ++y) hh[y] = 0;
		int numNotTransparent = 0;
		for (int x = 0; x < tw; ++x)
		{
			for (int y = 0; y < th; ++y)
			{
				Color col = srcTex.GetPixel(sx + x, sy + y);
				if (col.a > 0)
				{
					ww[x] = 1;
					hh[y] = 1;
					numNotTransparent++;
				}
			}
		}

		if (numNotTransparent > 0)
		{
			int x0 = 0, x1 = 0, y0 = 0, y1 = 0;
			
			bool customSpriteGeometry = false;
			if (!isInjectedTexture && settings.textureParams[spriteLut.source].customSpriteGeometry) { 
				customSpriteGeometry = true;
			}
			
			// For custom geometry, use the bounds of the geometry
			if (customSpriteGeometry)
			{
				var textureParams = settings.textureParams[spriteLut.source];
				
				x0 = int.MaxValue;
				y0 = int.MaxValue;
				x1 = -1;
				y1 = -1;
				
				foreach (var island in textureParams.geometryIslands)
				{
					foreach (Vector2 rawVert in island.points)
					{
						Vector2 vert = rawVert * settings.globalTextureRescale;
						int minX = Mathf.FloorToInt(vert.x);
						int maxX = Mathf.CeilToInt(vert.x);
						float y = th - vert.y;
						int minY = Mathf.FloorToInt(y);
						int maxY = Mathf.CeilToInt(y);
						
						x0 = Mathf.Min(x0, minX);
						y0 = Mathf.Min(y0, minY);
						x1 = Mathf.Max(x1, maxX);
						y1 = Mathf.Max(y1, maxY);
					}
				}
			}
			else
			{
				for (int x = 0; x < tw; ++x) if (ww[x] == 1) { x0 = x; break; }
				for (int x = tw - 1; x >= 0; --x) if (ww[x] == 1) { x1 = x; break; }
				for (int y = 0; y < th; ++y) if (hh[y] == 1) { y0 = y; break; }
				for (int y = th - 1; y >= 0; --y) if (hh[y] == 1) { y1 = y; break; }
			}
			
			x1 = Mathf.Min(x1, tw - 1);
			y1 = Mathf.Min(y1, th - 1);
	
			int w1 = x1 - x0 + 1;
			int h1 = y1 - y0 + 1;
			
			if (!allowTrimming)
			{
				x0 = 0;
				y0 = 0;
				w1 = tw;
				h1 = th;
			}
			
			Texture2D dtex = new Texture2D(w1 + padAmount * 2, h1 + padAmount * 2);
			dtex.hideFlags = HideFlags.DontSave;
			for (int x = 0; x < w1; ++x)
			{
				for (int y = 0; y < h1; ++y)
				{
					Color col = srcTex.GetPixel(sx + x0 + x, sy + y0 + y);
					dtex.SetPixel(x + padAmount, y + padAmount, col);
				}
			}
			
			if (settings.premultipliedAlpha)
			{
				for (int x = 0; x < dtex.width; ++x)
				{
					for (int y = 0; y < dtex.height; ++y)
					{
						Color col = dtex.GetPixel(x, y);
                        col.r *= col.a; col.g *= col.a; col.b *= col.a;
						col.a = additive?0.0f:col.a;
						dtex.SetPixel(x, y, col);
					}
				}
			}
			
			PadTexture(dtex, padAmount, padMode);
			switch (textureCompression)
			{
			case tk2dSpriteCollection.TextureCompression.Dithered16Bit_NoAlpha:
				tk2dEditor.TextureProcessing.FloydSteinbergDithering.DitherTexture(dtex, TextureFormat.ARGB4444, 0, 0, dtex.width, dtex.height); break;
			case tk2dSpriteCollection.TextureCompression.Dithered16Bit_Alpha:
				tk2dEditor.TextureProcessing.FloydSteinbergDithering.DitherTexture(dtex, TextureFormat.RGB565, 0, 0, dtex.width, dtex.height); break;
			}
			dtex.Apply();

			spriteLut.rx = sx + x0;
			spriteLut.ry = sy + y0;
			spriteLut.rw = w1;
			spriteLut.rh = h1;
			spriteLut.tex = dtex;

			return dtex;
		}
		else
		{
			return null;
		}
	}

	static tk2dSpriteCollection currentBuild = null;
	static Texture2D[] sourceTextures;
	
	// path is local (Assets/a/b/c.def)
	static void BuildDirectoryToFile(string localPath)
	{
		string basePath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		System.IO.FileInfo fileInfo = new System.IO.FileInfo(basePath + localPath);
		if (!fileInfo.Directory.Exists)
		{
			fileInfo.Directory.Create();
		}
	}

	// Create the sprite collection data object
	// Its a prefab for historic reasons
	static void CreateDataObject(tk2dSpriteCollection gen, string prefabObjectPath)
	{
        // Create prefab
		if (gen.spriteCollection == null)
		{
			prefabObjectPath = AssetDatabase.GenerateUniqueAssetPath(prefabObjectPath);
			
			GameObject go = new GameObject();
			go.AddComponent<tk2dSpriteCollectionData>();
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			Object p = EditorUtility.CreateEmptyPrefab(prefabObjectPath);
			EditorUtility.ReplacePrefab(go, p);
#else
			Object p = PrefabUtility.CreateEmptyPrefab(prefabObjectPath);
			PrefabUtility.ReplacePrefab(go, p);
#endif
			GameObject.DestroyImmediate(go);
			AssetDatabase.SaveAssets();

			gen.spriteCollection = AssetDatabase.LoadAssetAtPath(prefabObjectPath, typeof(tk2dSpriteCollectionData)) as tk2dSpriteCollectionData;
		}

	}

	public static string GetOrCreateDataPath(tk2dSpriteCollection gen)
	{
		if (gen.spriteCollection != null)
		{
			return System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(gen.spriteCollection));
		}
		else
		{
			string path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(gen)) + "/" + gen.name + " Data";
			if (!System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);
			return path;
		}	
	}

	static bool CheckSourceAssets(tk2dSpriteCollection gen)
	{
		string missingTextures = "";

		foreach (var param in gen.textureParams) {
			if (param.texture == null && param.name.Length > 0) {
				missingTextures += "  Missing texture: " + param.name;
			}
		}

		if (missingTextures.Length > 0) {
			Debug.LogError(string.Format("Error in sprite collection '{0}'\n{1}", gen.name, missingTextures));
		}

		return missingTextures.Length == 0;
	}

	static void SetSpriteLutHash(SpriteLut lut)
	{
		byte[] buf;
		if (lut.tex) {
			Color32[] pixelData = lut.tex.GetPixels32();
			int ptr = 0;
			buf = new byte[6 + pixelData.Length * 4];
			for (int i = 0; i < pixelData.Length; ++i) {
				buf[ptr++] = pixelData[i].r;
				buf[ptr++] = pixelData[i].g;
				buf[ptr++] = pixelData[i].b;
				buf[ptr++] = pixelData[i].a;
			}
			buf[ptr++] = (byte)((lut.tex.width & 0x000000ff));
			buf[ptr++] = (byte)((lut.tex.width & 0x0000ff00) >> 8);
			buf[ptr++] = (byte)((lut.tex.width & 0x00ff0000) >> 16);
			buf[ptr++] = (byte)((lut.tex.height & 0x000000ff));
			buf[ptr++] = (byte)((lut.tex.height & 0x0000ff00) >> 8);
			buf[ptr++] = (byte)((lut.tex.height & 0x00ff0000) >> 16);
		} else {
			buf = new byte[] { 0 };
		}
		MD5 md5Hash = MD5.Create();
		byte[] data = md5Hash.ComputeHash(buf);
		StringBuilder sBuilder = new StringBuilder(data.Length * 2);
		for (int i = 0; i < data.Length; ++i)
			sBuilder.Append(data[i].ToString("x2"));
		lut.hash = sBuilder.ToString();
	}

    public static bool Rebuild(tk2dSpriteCollection gen)
    {
		// avoid "recursive" build being triggered by texture watcher
		if (currentBuild != null)
			return false;

		// Version specific checks. These need to be done before the sprite collection is upgraded.
		if (gen.version < 2)
		{
	      	if (!tk2dEditor.SpriteCollectionBuilder.Deprecated.CheckAndFixUpParams(gen))
			{
				// Params failed check
				return false;
			}
			tk2dEditor.SpriteCollectionBuilder.Deprecated.SetUpSpriteSheets(gen);
			tk2dEditor.SpriteCollectionBuilder.Deprecated.TrimTextureList(gen);
		}
		currentBuild = gen;
		gen.Upgrade(); // upgrade if necessary. could be invoked by texture watcher.
		
		// Check all source assets are present, fail otherwise
		if (!CheckSourceAssets(gen)) {
			return false;
		}

		// Get some sensible paths to work with
		string dataDirName = GetOrCreateDataPath(gen) + "/";
		
		string prefabObjectPath = "";
		if (gen.spriteCollection)
			prefabObjectPath = AssetDatabase.GetAssetPath(gen.spriteCollection);
		else
			prefabObjectPath = dataDirName + gen.name + ".prefab";
		BuildDirectoryToFile(prefabObjectPath);

		// Create prefab object, needed for next stage
		CreateDataObject( gen, prefabObjectPath );

		// Special build for platform specific sprite collection
		if (gen.HasPlatformData)
		{
			// Initialize required sprite collections
			tk2dEditor.SpriteCollectionBuilder.PlatformBuilder.InitializeSpriteCollectionPlatforms(gen, System.IO.Path.GetDirectoryName(prefabObjectPath));

			// The first sprite collection is always THIS
			tk2dAssetPlatform baseAssetPlatform = tk2dSystem.GetAssetPlatform(gen.platforms[0].name);
			float baseScale = (baseAssetPlatform != null)?baseAssetPlatform.scale:1.0f;

			// Building platform specific data, allow recursive builds temporarily
			currentBuild = null;

			// Transfer to platform sprite collections, and build those
			for (int i = 0; i < gen.platforms.Count; ++i)
			{
				tk2dSpriteCollectionPlatform platform = gen.platforms[i];
				
				tk2dAssetPlatform thisAssetPlatform = tk2dSystem.GetAssetPlatform(gen.platforms[i].name);
				float thisScale = (thisAssetPlatform != null)?thisAssetPlatform.scale:1.0f;

				bool validPlatform = platform.name.Length > 0 && platform.spriteCollection != null;
				bool isRootSpriteCollection = (i == 0);
				if (validPlatform) 
				{
					tk2dSpriteCollection thisPlatformCollection = gen.platforms[i].spriteCollection;
					
					// Make sure data directory exists, material overrides will be created in here
					string dataPath = GetOrCreateDataPath(thisPlatformCollection);

					tk2dEditor.SpriteCollectionBuilder.PlatformBuilder.UpdatePlatformSpriteCollection(
						gen, thisPlatformCollection, dataPath, isRootSpriteCollection, thisScale / baseScale, platform.name);
					Rebuild(thisPlatformCollection);
				}
			}

			// Pull atlas data from default platform
			gen.atlasMaterials = gen.platforms[0].spriteCollection.atlasMaterials;
			gen.altMaterials = gen.platforms[0].spriteCollection.altMaterials;

			// Fill up our sprite collection data
			List<string> platformNames = new List<string>();
			List<string> platformGUIDs = new List<string>();

			for (int i = 0; i < gen.platforms.Count; ++i)
			{
				tk2dSpriteCollectionPlatform platform = gen.platforms[i];
				if (!platform.Valid) continue;

				platformNames.Add(platform.name);
				tk2dSpriteCollectionData data = platform.spriteCollection.spriteCollection;
				string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));
				platformGUIDs.Add(guid);

				// Make loadable
				tk2dSystemUtility.MakeLoadableAsset(data, ""); // unnamed loadable object
			}


			// Set up fonts
			for (int j = 0; j < gen.fonts.Length; ++j)
			{
				tk2dSpriteCollectionFont font = gen.fonts[j];
				if (font == null) continue;

				tk2dFontData data = font.data;
				if (!data) continue;
				data.hasPlatformData = true;
				data.material = null;
				data.materialInst = null;
				data.fontPlatforms = platformNames.ToArray();
				data.fontPlatformGUIDs = new string[platformNames.Count];
				data.premultipliedAlpha = gen.premultipliedAlpha;
				for (int i = 0; i < gen.platforms.Count; ++i)
				{
					tk2dSpriteCollectionPlatform platform = gen.platforms[i];
					if (!platform.Valid) continue;
					data.fontPlatformGUIDs[i] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gen.platforms[i].spriteCollection.fonts[j].data));
				}

				EditorUtility.SetDirty(data);
			}

			gen.spriteCollection.version = tk2dSpriteCollectionData.CURRENT_VERSION;
			gen.spriteCollection.spriteCollectionName = gen.name;
			gen.spriteCollection.spriteCollectionGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gen));
			gen.spriteCollection.dataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gen.spriteCollection));

			// Clear out data
			gen.spriteCollection.spriteDefinitions = new tk2dSpriteDefinition[0];
			gen.spriteCollection.materials = new Material[0];
			gen.spriteCollection.textures = new Texture2D[0];

			gen.spriteCollection.hasPlatformData = true;
			gen.spriteCollection.spriteCollectionPlatforms = platformNames.ToArray();
			gen.spriteCollection.spriteCollectionPlatformGUIDs = platformGUIDs.ToArray();
			gen.spriteCollection.ResetPlatformData();

			EditorUtility.SetDirty(gen);
			EditorUtility.SetDirty(gen.spriteCollection);

			// Index this properly
			tk2dEditorUtility.GetOrCreateIndex().AddSpriteCollectionData(gen.spriteCollection);
			tk2dEditorUtility.CommitIndex();

			AssetDatabase.SaveAssets();
			
			RefreshExistingAssets(gen.spriteCollection);
			
			ResetCurrentBuild();
			return true;
		}
		else
		{
			// clear platform data
			gen.spriteCollection.ResetPlatformData(); // in case its being used
			gen.spriteCollection.hasPlatformData = false;
			gen.spriteCollection.spriteCollectionPlatforms = new string[0];
			gen.spriteCollection.spriteCollectionPlatformGUIDs = new string[0];

			// Set up fonts
			for (int j = 0; j < gen.fonts.Length; ++j)
			{
				tk2dFontData f = gen.fonts[j].data;
				if (f != null)
				{
					f.ResetPlatformData();
					f.hasPlatformData = false;
					f.fontPlatforms = new string[0];
					f.fontPlatformGUIDs = new string[0];
					EditorUtility.SetDirty(f);
				}
			}
		}

		// Make sure all source textures are in the correct format
		SetUpSourceTextureFormats(gen);

		// blank texture used when texture has been deleted
		Texture2D blankTexture = new Texture2D(2, 2);
		blankTexture.hideFlags = HideFlags.DontSave;
		blankTexture.SetPixel(0, 0, Color.magenta);
		blankTexture.SetPixel(0, 1, Color.yellow);
		blankTexture.SetPixel(1, 0, Color.cyan);
		blankTexture.SetPixel(1, 1, Color.grey);
		blankTexture.Apply();
		
		// make local texture sources
		List<Texture2D> allocatedTextures = new List<Texture2D>();
		allocatedTextures.Add( blankTexture );

		// If globalTextureRescale is 0.5 or 0.25, average pixels from the larger image. Otherwise just pick one pixel, and look really bad
		Texture2D[] rescaledTexs = null;
		if (gen.globalTextureRescale < 0.999f) {
			rescaledTexs = new Texture2D[gen.textureParams.Length];
			for (int i = 0; i < gen.textureParams.Length; ++i) {
				if (gen.textureParams[i] != null 
					&& !gen.textureParams[i].extractRegion
					&& gen.textureParams[i].texture != null) {
					rescaledTexs[i] = tk2dSpriteCollectionBuilderUtil.RescaleTexture( gen.textureParams[i].texture, gen.globalTextureRescale );
					allocatedTextures.Add(rescaledTexs[i]);
				}
			}
		}
		else {
			gen.globalTextureRescale = 1;
		}

		Dictionary<Texture2D, Texture2D> extractRegionCache = new Dictionary<Texture2D, Texture2D>();
		sourceTextures = new Texture2D[gen.textureParams.Length];
		for (int i = 0; i < gen.textureParams.Length; ++i)
		{
			var param = gen.textureParams[i];
			if (param.extractRegion && param.texture != null)
			{
				Texture2D srcTex = param.texture;
				if (rescaledTexs != null) {
					if (!extractRegionCache.TryGetValue(param.texture, out srcTex)) {
						srcTex = tk2dSpriteCollectionBuilderUtil.RescaleTexture(param.texture, gen.globalTextureRescale);
						extractRegionCache[param.texture] = srcTex;
					}
				}

				int regionX = param.regionX;
				int regionY = param.regionY;
				int regionW = param.regionW;
				int regionH = param.regionH;
				if (rescaledTexs != null) {
					int k = tk2dSpriteCollectionBuilderUtil.NiceRescaleK( gen.globalTextureRescale );
					int x2, y2;
					if (k != 0) {
						regionX /= k;
						regionY /= k;
						x2 = regionX + (regionW + k - 1) / k;
						y2 = regionY + (regionH + k - 1) / k;
					} else {
						x2 = (int)((regionX + regionW) * gen.globalTextureRescale);
						y2 = (int)((regionY + regionH) * gen.globalTextureRescale);
						regionX = (int)(regionX * gen.globalTextureRescale);
						regionY = (int)(regionY * gen.globalTextureRescale);
					}
					regionW = Mathf.Min(x2, srcTex.width - 1) - regionX;
					regionH = Mathf.Min(y2, srcTex.height - 1) - regionY;
				}
				Texture2D localTex = new Texture2D(regionW, regionH);
				localTex.hideFlags = HideFlags.DontSave;
				for (int y = 0; y < regionH; ++y)
				{
					for (int x = 0; x < regionW; ++x)
					{
						localTex.SetPixel(x, y, srcTex.GetPixel(regionX + x, regionY + y));
					}
				}
				localTex.name = param.texture.name + "/" + param.regionId.ToString();
				localTex.Apply();
				allocatedTextures.Add(localTex);
				sourceTextures[i] = localTex;
			}
			else
			{
				sourceTextures[i] = (rescaledTexs != null) ? rescaledTexs[i] : param.texture;
			}
		}
		// Clear the region cache
		foreach (Texture2D tex in extractRegionCache.Values) {
			Object.DestroyImmediate(tex);
		}
		extractRegionCache = null;

		// catalog all textures to atlas
		int numTexturesToAtlas = 0;
		List<SpriteLut> spriteLuts = new List<SpriteLut>();
		for (int i = 0; i < gen.textureParams.Length; ++i)
		{
			Texture2D currentTexture = sourceTextures[i];

			if (sourceTextures[i] == null)
			{
				gen.textureParams[i].dice = false;
				gen.textureParams[i].anchor = tk2dSpriteCollectionDefinition.Anchor.MiddleCenter;
				gen.textureParams[i].name = "";
				gen.textureParams[i].extractRegion = false;
				gen.textureParams[i].fromSpriteSheet = false;

				currentTexture = blankTexture;
			}
			else
			{
				if (gen.textureParams[i].name == null || gen.textureParams[i].name == "")
				{
					if (gen.textureParams[i].texture != currentTexture && !gen.textureParams[i].fromSpriteSheet)
					{
						gen.textureParams[i].name = currentTexture.name;
					}
				}
			}

			if (gen.textureParams[i].dice)
			{
				// prepare to dice this up
				Texture2D srcTex = currentTexture;
				int diceUnitX = (int)( gen.textureParams[i].diceUnitX * gen.globalTextureRescale );
				int diceUnitY = (int)( gen.textureParams[i].diceUnitY * gen.globalTextureRescale );
				if (diceUnitX <= 0) diceUnitX = srcTex.width; // something sensible, please
				if (diceUnitY <= 0) diceUnitY = srcTex.height; // make square if not set

				for (int sx = 0; sx < srcTex.width; sx += diceUnitX)
				{
					for (int sy = 0; sy < srcTex.height; sy += diceUnitY)
					{
						int tw = Mathf.Min(diceUnitX, srcTex.width - sx);
						int th = Mathf.Min(diceUnitY, srcTex.height - sy);

						if (gen.textureParams[i].diceFilter == tk2dSpriteCollectionDefinition.DiceFilter.SolidOnly &&
							!TextureRectFullySolid( srcTex, sx, sy, tw, th )) {
							continue;
						}

						if (gen.textureParams[i].diceFilter == tk2dSpriteCollectionDefinition.DiceFilter.TransparentOnly &&
							TextureRectFullySolid( srcTex, sx, sy, tw, th )) {
							continue;
						}

						SpriteLut diceLut = new SpriteLut();
						diceLut.source = i;
						diceLut.isSplit = true;
						diceLut.sourceTex = srcTex;
						diceLut.isDuplicate = false; // duplicate diced textures can be chopped up differently, so don't detect dupes here

						Texture2D dest = ProcessTexture(gen, gen.textureParams[i].additive, tk2dSpriteCollectionDefinition.Pad.Extend, gen.textureParams[i].disableTrimming, false, srcTex, sx, sy, tw, th, ref diceLut, GetPadAmount(gen, i));
						if (dest)
						{
							diceLut.atlasIndex = numTexturesToAtlas++;
							spriteLuts.Add(diceLut);
						}
					}
				}
			}
			else
			{
				SpriteLut lut = new SpriteLut();
				lut.sourceTex = currentTexture;
				lut.source = i;

				lut.isSplit = false;
				lut.isDuplicate = false;
				for (int j = 0; j < spriteLuts.Count; ++j)
				{
					if (spriteLuts[j].sourceTex == lut.sourceTex)
					{
						lut.isDuplicate = true;
						lut.atlasIndex = spriteLuts[j].atlasIndex;
						lut.tex = spriteLuts[j].tex; // get old processed tex

						lut.rx = spriteLuts[j].rx; lut.ry = spriteLuts[j].ry;
						lut.rw = spriteLuts[j].rw; lut.rh = spriteLuts[j].rh;

						break;
					}
				}

				if (!lut.isDuplicate)
				{
					lut.atlasIndex = numTexturesToAtlas++;
					Texture2D dest = ProcessTexture(gen, gen.textureParams[i].additive, gen.textureParams[i].pad, gen.textureParams[i].disableTrimming, false, currentTexture, 0, 0, currentTexture.width, currentTexture.height, ref lut, GetPadAmount(gen, i));
					if (dest == null)
					{
						// fall back to a tiny blank texture
						lut.tex = new Texture2D(1, 1);
						lut.tex.hideFlags = HideFlags.DontSave;
						lut.tex.SetPixel(0, 0, new Color( 0, 0, 0, 0 ));
						PadTexture(lut.tex, GetPadAmount(gen, i), gen.textureParams[i].pad);
						lut.tex.Apply();

						lut.rx = currentTexture.width / 2; lut.ry = currentTexture.height / 2;
						lut.rw = 1; lut.rh = 1;
					}
				}

				spriteLuts.Add(lut);
			}
		}
		
		// Font
		Dictionary<tk2dSpriteCollectionFont, tk2dEditor.Font.Info> fontInfoDict = new Dictionary<tk2dSpriteCollectionFont, tk2dEditor.Font.Info>();
		if (gen.fonts != null)
		{
			for (int i = 0; i < gen.fonts.Length; ++i)
			{
				var font = gen.fonts[i];
				if (!font.InUse) continue;
				
				float texScale = gen.globalTextureRescale;					
				Texture2D rescaledTexture = ( texScale < 1 ) ? tk2dSpriteCollectionBuilderUtil.RescaleTexture( font.texture, gen.globalTextureRescale ) : null;

				tk2dEditor.Font.Info fontInfo = tk2dEditor.Font.Builder.ParseBMFont( AssetDatabase.GetAssetPath(font.bmFont) );
				fontInfoDict[font] = fontInfo;

				// need to allow this to compensate for rescaled textures later.
				if (rescaledTexture != null) {
					fontInfo.textureScale = texScale;
				}

				foreach (var c in fontInfo.chars)
				{
					// skip empty textures
					if (c.width <= 0 || c.height <= 0)
						continue;
					
					SpriteLut lut = new SpriteLut();

					int cy = (int)( (font.flipTextureY ? c.y : (fontInfo.scaleH - c.y - c.height)) * texScale );
					Texture2D dest = ProcessTexture(gen, false, tk2dSpriteCollectionDefinition.Pad.Default, false, true,
						(rescaledTexture != null) ? rescaledTexture : font.texture, 
						(int)(c.x * texScale), cy, 
						(int)(c.width * texScale), (int)(c.height * texScale), 
						ref lut, GetPadAmount(gen, -1));
					if (dest == null)
					{
						// probably fully transparent
						continue;
					}
					
					lut.sourceTex = dest;
					lut.tex = dest;
					lut.source = -1;
					lut.isFont = true;
					lut.isDuplicate = false;
					lut.fontId = i;
					lut.charId = c.id;
					lut.rx = lut.rx - (int)(c.x * texScale);
					lut.ry = lut.ry - cy;
					
					lut.atlasIndex = numTexturesToAtlas++;

					spriteLuts.Add(lut);
				}

				// Free tmp allocated texture
				if (rescaledTexture != null) {
					Texture2D.DestroyImmediate( rescaledTexture );
				}
				
				// Add one blank char for fallbacks
				{
					int dims = 5;
					SpriteLut lut = new SpriteLut();
					lut.tex = new Texture2D(dims, dims);
					lut.tex.hideFlags = HideFlags.DontSave;
					for (int y = 0; y < dims; ++y)
						for (int x = 0; x < dims; ++x)
							lut.tex.SetPixel(x, y, Color.clear);
					lut.tex.Apply();
					lut.sourceTex = lut.tex;
					lut.isFont = true;
					lut.isDuplicate = false;
					lut.fontId = i;
					lut.charId = -1;
					lut.atlasIndex = numTexturesToAtlas++;
					
					spriteLuts.Add(lut);
				}
			}
		}

		if (gen.removeDuplicates) {

			//System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

			// Set texture hashes on SpriteLuts
			foreach (var lut in spriteLuts) {
				SetSpriteLutHash(lut);
			}

			//sw.Stop();
			//Debug.Log(string.Format("Time: {0}ms", sw.Elapsed.TotalMilliseconds));

			// Find more duplicates based on the hash
			for (int i = 0; i < spriteLuts.Count; ++i) {
				for (int j = i + 1; j < spriteLuts.Count; ++j) {
					if (!spriteLuts[j].isDuplicate) {
						if (spriteLuts[i].hash == spriteLuts[j].hash) {
							spriteLuts[j].isDuplicate = true;
							Object.DestroyImmediate(spriteLuts[j].tex);

							foreach (var lut in spriteLuts) {
								if (lut.atlasIndex > spriteLuts[j].atlasIndex)
									--lut.atlasIndex;
							}
							--numTexturesToAtlas;

							spriteLuts[j].atlasIndex = spriteLuts[i].atlasIndex;
						}
					}
				}
			}

		}

        // Create texture
		Texture2D[] textureList = new Texture2D[numTexturesToAtlas];
        int titer = 0;
        for (int i = 0; i < spriteLuts.Count; ++i)
        {
			SpriteLut _lut = spriteLuts[i];
			if (!_lut.isDuplicate)
			{
				textureList[titer++] = _lut.tex;
			}
        }
		
		// Build atlas
		bool forceAtlasSize = gen.forceTextureSize;
		int atlasWidth = forceAtlasSize?gen.forcedTextureWidth:gen.maxTextureSize;
		int atlasHeight = forceAtlasSize?gen.forcedTextureHeight:gen.maxTextureSize;
		bool forceSquareAtlas = forceAtlasSize?false:gen.forceSquareAtlas;
		bool allowFindingOptimalSize = !forceAtlasSize;
		tk2dEditor.Atlas.Builder atlasBuilder = new tk2dEditor.Atlas.Builder(atlasWidth, atlasHeight, gen.allowMultipleAtlases?64:1, allowFindingOptimalSize, forceSquareAtlas);
		if (textureList.Length > 0)
		{
			foreach (Texture2D currTexture in textureList)
			{
				atlasBuilder.AddRect(currTexture.width, currTexture.height);
			}
			if (atlasBuilder.Build() != 0)
			{
				if (atlasBuilder.HasOversizeTextures())
				{
					EditorUtility.DisplayDialog("Unable to fit in atlas",
					                            "You have a texture which exceeds the atlas size. " +
					                            "Consider putting it in a separate atlas, enabling dicing, or " +
					                            "reducing the texture size",
								                "Ok");
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to fit textures in requested atlas area",
					                            "There are too many textures in this collection for the requested " +
					                            "atlas size.",
								                "Ok");
				}
				return false;
			}
		}
		


		// Fill atlas textures
		List<Material> oldAtlasMaterials = new List<Material>(gen.atlasMaterials);
		List<Texture2D> oldAtlasTextures = new List<Texture2D>(gen.atlasTextures);

		tk2dEditor.Atlas.Data[] atlasData = atlasBuilder.GetAtlasData();
		System.Array.Resize(ref gen.atlasTextures, atlasData.Length);
		System.Array.Resize(ref gen.atlasMaterials, atlasData.Length);
		if (atlasData.Length > 1)
		{
			// wipe out alt materials when atlas spanning is on
			gen.altMaterials = new Material[0];
		}
		for (int atlasIndex = 0; atlasIndex < atlasData.Length; ++atlasIndex)
		{
	        Texture2D tex = new Texture2D(atlasData[atlasIndex].width, atlasData[atlasIndex].height, TextureFormat.ARGB32, false);
	        tex.hideFlags = HideFlags.DontSave;
			gen.atlasWastage = (1.0f - atlasData[0].occupancy) * 100.0f;
			gen.atlasWidth = atlasData[0].width;
			gen.atlasHeight = atlasData[0].height;

			// Clear texture, unsure if this is REALLY necessary
			// Turns out it is
			for (int yy = 0; yy < tex.height; ++yy)
			{
				for (int xx = 0; xx < tex.width; ++xx)
				{
					tex.SetPixel(xx, yy, Color.clear);
				}
			}

			for (int i = 0; i < atlasData[atlasIndex].entries.Length; ++i)
			{
				var entry = atlasData[atlasIndex].entries[i];
				Texture2D source = textureList[entry.index];

				if (!entry.flipped)
				{
					for (int y = 0; y < source.height; ++y)
					{
						for (int x = 0; x < source.width; ++x)
						{
							tex.SetPixel(entry.x + x, entry.y + y, source.GetPixel(x, y));
						}
					}
				}
				else
				{
					for (int y = 0; y < source.height; ++y)
					{
						for (int x = 0; x < source.width; ++x)
						{
							tex.SetPixel(entry.x + y, entry.y + x, source.GetPixel(x, y));
						}
					}
				}
			}
			tex.Apply();

			string texturePath = gen.atlasTextures[atlasIndex]?AssetDatabase.GetAssetPath(gen.atlasTextures[atlasIndex]):(dataDirName + "atlas" + atlasIndex + ".png");
			BuildDirectoryToFile(texturePath);

			// Write filled atlas to disk
			byte[] bytes = tex.EncodeToPNG();
			System.IO.FileStream fs = System.IO.File.Create(texturePath);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();

			Object.DestroyImmediate(tex);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Get a reference to the texture asset
			tex = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
			gen.atlasTextures[atlasIndex] = tex;

			// Make sure texture is set up with the correct max size and compression type
			SetUpTargetTexture(gen, tex);

	        // Create material if necessary
	        if (gen.atlasMaterials[atlasIndex] == null)
	        {
				Material mat;
	            if (gen.premultipliedAlpha)
	                mat = new Material(Shader.Find("tk2d/PremulVertexColor"));
	            else
	                mat = new Material(Shader.Find("tk2d/BlendVertexColor"));

				mat.mainTexture = tex;

				string materialPath = gen.atlasMaterials[atlasIndex]?AssetDatabase.GetAssetPath(gen.atlasMaterials[atlasIndex]):(dataDirName + "atlas" + atlasIndex + " material.mat");
				BuildDirectoryToFile(materialPath);
				
	            AssetDatabase.CreateAsset(mat, materialPath);
				AssetDatabase.SaveAssets();

				gen.atlasMaterials[atlasIndex] = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
			}
			
			// gen.altMaterials must either have length 0, or contain at least the material used in the game
			if (!gen.allowMultipleAtlases && (gen.altMaterials == null || gen.altMaterials.Length == 0))
				gen.altMaterials = new Material[1] { gen.atlasMaterials[0] };
		}

		tk2dSpriteCollectionData coll = gen.spriteCollection;
		coll.textures = new Texture[gen.atlasTextures.Length];
		for (int i = 0; i < gen.atlasTextures.Length; ++i)
		{
			coll.textures[i] = gen.atlasTextures[i];
		}
		
		if (!gen.allowMultipleAtlases && gen.altMaterials.Length > 1)
		{
			coll.materials = new Material[gen.altMaterials.Length];
	        for (int i = 0; i < gen.altMaterials.Length; ++i)
				coll.materials[i] = gen.altMaterials[i];
		}
		else
		{
			coll.materials = new Material[gen.atlasMaterials.Length];
	        for (int i = 0; i < gen.atlasMaterials.Length; ++i)
				coll.materials[i] = gen.atlasMaterials[i];
		}
		
		// Delete unused atlas textures & materials
		DeleteUnusedAssets( oldAtlasMaterials, gen.atlasMaterials );
		DeleteUnusedAssets( oldAtlasTextures, gen.atlasTextures );
		
		// Wipe out legacy data
		coll.material = null;
		
        coll.premultipliedAlpha = gen.premultipliedAlpha;
        coll.spriteDefinitions = new tk2dSpriteDefinition[gen.textureParams.Length];
		coll.version = tk2dSpriteCollectionData.CURRENT_VERSION;
		coll.materialIdsValid = true;
		coll.spriteCollectionGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(gen));
		coll.spriteCollectionName = gen.name;

		int buildKey = Random.Range(0, int.MaxValue);
		while (buildKey == coll.buildKey)
		{
			buildKey = Random.Range(0, int.MaxValue);
		}
		coll.buildKey = buildKey; // a random build number so we can identify changed collections quickly
		
		for (int i = 0; i < coll.spriteDefinitions.Length; ++i)
		{
			coll.spriteDefinitions[i] = new tk2dSpriteDefinition();
			if (gen.textureParams[i].texture)
			{
				string assetPath = AssetDatabase.GetAssetPath(gen.textureParams[i].texture);
				string guid = AssetDatabase.AssetPathToGUID(assetPath);
				coll.spriteDefinitions[i].sourceTextureGUID = guid;
			}
			else
			{
				coll.spriteDefinitions[i].sourceTextureGUID = "";
			}
		
			coll.spriteDefinitions[i].extractRegion = gen.textureParams[i].extractRegion;
			coll.spriteDefinitions[i].regionX = gen.textureParams[i].regionX;
			coll.spriteDefinitions[i].regionY = gen.textureParams[i].regionY;
			coll.spriteDefinitions[i].regionW = gen.textureParams[i].regionW;
			coll.spriteDefinitions[i].regionH = gen.textureParams[i].regionH;
		}
		coll.allowMultipleAtlases = gen.allowMultipleAtlases;
		coll.dataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(coll));
		
		float scale = 1.0f;
		coll.invOrthoSize = 1.0f / gen.sizeDef.OrthoSize;
		coll.halfTargetHeight = 0.5f * gen.sizeDef.TargetHeight;
		scale = (2.0f * gen.sizeDef.OrthoSize / gen.sizeDef.TargetHeight) * gen.globalScale / gen.globalTextureRescale;
		
		// Build fonts
		foreach (var font in fontInfoDict.Keys)
		{
			var fontInfo = fontInfoDict[font];
			List<SpriteLut> fontSpriteLut = new List<SpriteLut>();
			int fontId = 0;
			for (fontId = 0; fontId < gen.fonts.Length; ++fontId)
				if (gen.fonts[fontId] == font) break;
			foreach (var v in spriteLuts)
			{
				if (v.isFont && v.fontId == fontId)
					fontSpriteLut.Add(v);
			}
			
			fontInfo.scaleW = coll.textures[0].width;
			fontInfo.scaleH = coll.textures[0].height;
			
			// Set material
			if (font.useGradient && font.gradientTexture != null && font.gradientCount > 0)
			{
				font.editorData.gradientCount = font.gradientCount;
				font.editorData.gradientTexture = font.gradientTexture;
			}
			else
			{
				font.editorData.gradientCount = 1;
				font.editorData.gradientTexture = null;
			}

			// Build a local sprite lut only relevant to this font
			UpdateFontData(gen, scale * gen.globalTextureRescale, atlasData, fontSpriteLut, font, fontInfo);
			
			if (font.useGradient && font.gradientTexture != null)
			{
				font.data.gradientCount = font.editorData.gradientCount;
				font.data.gradientTexture = font.editorData.gradientTexture;
				font.data.textureGradients = true;				
			}
			else
			{
				font.data.gradientCount = 1;
				font.data.gradientTexture = null;
				font.data.textureGradients = false;
			}

			font.data.premultipliedAlpha = gen.premultipliedAlpha;
			font.data.spriteCollection = gen.spriteCollection;
			font.data.material = coll.materials[font.materialId];
			font.editorData.material = coll.materials[font.materialId];

			font.data.invOrthoSize = coll.invOrthoSize;
			font.data.halfTargetHeight = coll.halfTargetHeight;
			font.data.texelSize = new Vector3(scale / gen.globalScale, scale / gen.globalScale, 0.0f);

			// Managed?
			font.data.managedFont = gen.managedSpriteCollection;
			font.data.needMaterialInstance = gen.managedSpriteCollection;

			// Mark to save
			EditorUtility.SetDirty(font.editorData);
			EditorUtility.SetDirty(font.data);

			// Update font
			tk2dEditorUtility.GetOrCreateIndex().AddOrUpdateFont(font.editorData);
			tk2dEditorUtility.CommitIndex();

			// Loadable?
			if (font.editorData.loadable || font.data.managedFont)
				tk2dSystemUtility.MakeLoadableAsset(font.data, "");
		}
		
		// Build textures
		UpdateVertexCache(gen, scale, atlasData, coll, spriteLuts);

		// Free tmp textures
		foreach (var sprite in spriteLuts) {
			if (!sprite.isDuplicate) {
				Object.DestroyImmediate(sprite.tex);
			}
		}
		foreach (var tex in allocatedTextures) {
			Object.DestroyImmediate(tex);
		}
	
        // refresh existing
		gen.spriteCollection.ResetPlatformData();
		RefreshExistingAssets(gen.spriteCollection);
		
		// save changes
		gen.spriteCollection.loadable = gen.loadable;
		gen.spriteCollection.assetName = gen.assetName;
		gen.spriteCollection.managedSpriteCollection = gen.managedSpriteCollection;
		gen.spriteCollection.needMaterialInstance = gen.managedSpriteCollection;

		var index = tk2dEditorUtility.GetOrCreateIndex();
		index.AddSpriteCollectionData(gen.spriteCollection);
		EditorUtility.SetDirty(gen.spriteCollection);
		EditorUtility.SetDirty(gen);

		sourceTextures = null; // need to clear, its static
		currentBuild = null;
		
		tk2dEditorUtility.GetOrCreateIndex().AddSpriteCollectionData(gen.spriteCollection);
		tk2dEditorUtility.CommitIndex();
	
		// update resource system
		if (gen.spriteCollection.loadable)
		{
			tk2dSystemUtility.UpdateAssetName(gen.spriteCollection, gen.assetName);
		}
		
		return true;
    }
	
	// pass null to rebuild everything
	static void RefreshExistingAssets(tk2dSpriteCollectionData spriteCollectionData)
	{
		foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				System.Type[] types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.GetInterface("tk2dRuntime.ISpriteCollectionForceBuild") != null)
					{
						Object[] objects = Resources.FindObjectsOfTypeAll(type);
						foreach (var o in objects)
						{
							if (tk2dEditorUtility.IsPrefab(o))
								continue;
							
							tk2dRuntime.ISpriteCollectionForceBuild isc = o as tk2dRuntime.ISpriteCollectionForceBuild;
							if (spriteCollectionData == null || isc.UsesSpriteCollection(spriteCollectionData))
								isc.ForceBuild();
						}
					}
				}
			}
			catch { }
		}
	}

	static void SetUpTargetTexture(tk2dSpriteCollection gen, Texture2D tex)
	{
		bool textureDirty = false;

		string targetTexPath = AssetDatabase.GetAssetPath(tex);
		TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(targetTexPath);
		if (gen.maxTextureSize != importer.maxTextureSize)
		{
			importer.maxTextureSize = gen.maxTextureSize;
			textureDirty = true;
		}
		TextureImporterFormat targetFormat;
		switch (gen.textureCompression)
		{
		case tk2dSpriteCollection.TextureCompression.Uncompressed: targetFormat = TextureImporterFormat.AutomaticTruecolor; break;
		case tk2dSpriteCollection.TextureCompression.Reduced16Bit: targetFormat = TextureImporterFormat.Automatic16bit; break;
		case tk2dSpriteCollection.TextureCompression.Dithered16Bit_Alpha: targetFormat = TextureImporterFormat.Automatic16bit; break;
		case tk2dSpriteCollection.TextureCompression.Dithered16Bit_NoAlpha: targetFormat = TextureImporterFormat.Automatic16bit; break;
		case tk2dSpriteCollection.TextureCompression.Compressed: targetFormat = TextureImporterFormat.AutomaticCompressed; break;

		default: targetFormat = TextureImporterFormat.AutomaticTruecolor; break;
		}

		if (targetFormat != importer.textureFormat)
		{
			importer.textureFormat = targetFormat;
			textureDirty = true;
		}

		if (importer.filterMode != gen.filterMode) 
		{ 
			importer.filterMode = gen.filterMode; textureDirty = true; 
		}

		if (!gen.userDefinedTextureSettings)
		{
			if (importer.wrapMode != gen.wrapMode) { importer.wrapMode = gen.wrapMode; textureDirty = true; }
			if (importer.mipmapEnabled != gen.mipmapEnabled) { importer.mipmapEnabled = gen.mipmapEnabled; textureDirty = true; }
			if (importer.anisoLevel != gen.anisoLevel) { importer.anisoLevel = gen.anisoLevel; textureDirty = true; }
		}

		if (textureDirty)
		{
			EditorUtility.SetDirty(importer);
			AssetDatabase.ImportAsset(targetTexPath);
		}
	}
	
	static void UpdateFontData(tk2dSpriteCollection gen, float scale, tk2dEditor.Atlas.Data[] packers, List<SpriteLut> spriteLuts, tk2dSpriteCollectionFont font, tk2dEditor.Font.Info fontInfo)
	{
		Dictionary<int, SpriteLut> glyphLut = new Dictionary<int, SpriteLut>();
		List<SpriteLut> values = new List<SpriteLut>();
		foreach (var k in glyphLut.Keys) values.Add(glyphLut[k]);
		
		foreach (var v in spriteLuts)
			glyphLut[v.charId] = v;
		int padAmount = GetPadAmount(gen, -1);
		foreach (var c in fontInfo.chars)
		{
			if (glyphLut.ContainsKey(c.id))
			{
				var glyphSprite = glyphLut[c.id];
				var atlasEntry = packers[0].FindEntryWithIndex(glyphSprite.atlasIndex);
				
				c.texOffsetX = glyphSprite.rx;

				if (font.flipTextureY)
				{
					// This is the offset from the bottom of the font
					c.texOffsetY = glyphSprite.ry;
				}
				else
				{
					// ry is offset from top, we want offset from bottom
					// height is the original glyph height before cropping, the remainder of which is
					// the offset from bottom
					c.texOffsetY = c.height - glyphSprite.rh - glyphSprite.ry;
				}
				
				c.texX = atlasEntry.x + padAmount;
				c.texY = atlasEntry.y + padAmount;
				c.texW = atlasEntry.w - padAmount * 2;
				c.texH = atlasEntry.h - padAmount * 2;
				c.texFlipped = atlasEntry.flipped;
			}
			else
			{
				var glyphSprite = glyphLut[-1];
				var atlasEntry = packers[0].FindEntryWithIndex(glyphSprite.atlasIndex);
				
				c.texOffsetX = 0;
				c.texOffsetY = 0;
				
				c.texX = atlasEntry.x + 1;
				c.texY = atlasEntry.y + 1;
				c.texW = 0;
				c.texH = 0;
				c.texFlipped = false;
			}
			c.texOverride = true;
		}
		
		tk2dEditor.Font.Builder.BuildFont(fontInfo, font.data, scale, font.charPadX, font.dupeCaps, font.flipTextureY, font.gradientTexture, font.gradientCount);
	}
	
    static void UpdateVertexCache(tk2dSpriteCollection gen, float scale, tk2dEditor.Atlas.Data[] packers, tk2dSpriteCollectionData coll, List<SpriteLut> spriteLuts)
    {
        for (int i = 0; i < sourceTextures.Length; ++i)
        {
			SpriteLut _lut = null;
			for (int j = 0; j < spriteLuts.Count; ++j)
			{
				if (spriteLuts[j].source == i)
				{
					_lut = spriteLuts[j];
					break;
				}
			}

			int padAmount = GetPadAmount(gen, i);

            tk2dSpriteCollectionDefinition thisTexParam = gen.textureParams[i];
			tk2dEditor.Atlas.Data packer = packers[0];
			tk2dEditor.Atlas.Entry atlasEntry = null;
			int atlasIndex = 0;
			if (_lut != null) {
				foreach (var p in packers) {
					if ((atlasEntry = p.FindEntryWithIndex(_lut.atlasIndex)) != null) {
						packer = p;
						break;
					}
					++atlasIndex;
				}
			}
			float fwidth = packer.width;
    	    float fheight = packer.height;

    	    int tx = 0, ty = 0, tw = 0, th = 0;
    	    if (atlasEntry != null) {
            	tx = atlasEntry.x + padAmount;
            	ty = atlasEntry.y + padAmount;
            	tw = atlasEntry.w - padAmount * 2;
            	th = atlasEntry.h - padAmount * 2;
            }
            int sd_y = packer.height - ty - th;

			float uvOffsetX = 0.001f / fwidth;
			float uvOffsetY = 0.001f / fheight;

            Vector2 v0 = new Vector2(tx / fwidth + uvOffsetX, 1.0f - (sd_y + th) / fheight + uvOffsetY);
            Vector2 v1 = new Vector2((tx + tw) / fwidth - uvOffsetX, 1.0f - sd_y / fheight - uvOffsetY);

            Mesh mesh = null;
            Transform meshTransform = null;
            GameObject instantiated = null;
			
			Vector3 colliderOrigin = new Vector3();

            if (thisTexParam.overrideMesh)
            {
				// Disabled
                instantiated = GameObject.Instantiate(thisTexParam.overrideMesh) as GameObject;
                MeshFilter meshFilter = instantiated.GetComponentInChildren<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogError("Unable to find mesh");
                    GameObject.DestroyImmediate(instantiated);
                }
                else
                {
                    mesh = meshFilter.sharedMesh;
                    meshTransform = meshFilter.gameObject.transform;
                }
            }
			
			Vector3 untrimmedPos0 = Vector3.zero, untrimmedPos1 = Vector3.one;
			
            if (mesh)
            {
                coll.spriteDefinitions[i].positions = new Vector3[mesh.vertices.Length];
                coll.spriteDefinitions[i].uvs = new Vector2[mesh.vertices.Length];
                for (int j = 0; j < mesh.vertices.Length; ++j)
                {
                    coll.spriteDefinitions[i].positions[j] = meshTransform.TransformPoint(mesh.vertices[j]);
                    coll.spriteDefinitions[i].uvs[j] = new Vector2(v0.x + (v1.x - v0.x) * mesh.uv[j].x, v0.y + (v1.y - v0.y) * mesh.uv[j].y);
                }
                coll.spriteDefinitions[i].indices = new int[mesh.triangles.Length];
                for (int j = 0; j < mesh.triangles.Length; ++j)
                {
                    coll.spriteDefinitions[i].indices[j] = mesh.triangles[j];
                }
                GameObject.DestroyImmediate(instantiated);
            }
            else
            {
				Texture2D thisTextureRef = sourceTextures[i];
				
				int texHeightI = thisTextureRef?thisTextureRef.height:2;
				int texWidthI = thisTextureRef?thisTextureRef.width:2;
				float texHeight = texHeightI;
       			float texWidth = texWidthI;

				float h = thisTextureRef?thisTextureRef.height:64;
				float w = thisTextureRef?thisTextureRef.width:64;
                h *= thisTexParam.scale.y;
                w *= thisTexParam.scale.x;

				float scaleX = w * scale;
                float scaleY = h * scale;
				
				float anchorX = 0, anchorY = 0;

				// anchor coordinate system is (0, 0) = top left, to keep it the same as photoshop, etc.
                switch (thisTexParam.anchor)
                {
                    case tk2dSpriteCollectionDefinition.Anchor.LowerLeft: anchorX = 0; anchorY = texHeightI; break;
                    case tk2dSpriteCollectionDefinition.Anchor.LowerCenter: anchorX = texWidthI / 2; anchorY = texHeightI; break;
                    case tk2dSpriteCollectionDefinition.Anchor.LowerRight: anchorX = texWidthI; anchorY = texHeightI; break;

                    case tk2dSpriteCollectionDefinition.Anchor.MiddleLeft: anchorX = 0; anchorY = texHeightI / 2; break;
                    case tk2dSpriteCollectionDefinition.Anchor.MiddleCenter: anchorX = texWidthI / 2; anchorY = texHeightI / 2; break;
                    case tk2dSpriteCollectionDefinition.Anchor.MiddleRight: anchorX = texWidthI; anchorY = texHeightI / 2; break;

                    case tk2dSpriteCollectionDefinition.Anchor.UpperLeft: anchorX = 0; anchorY = 0; break;
                    case tk2dSpriteCollectionDefinition.Anchor.UpperCenter: anchorX = texWidthI / 2; anchorY = 0; break;
                    case tk2dSpriteCollectionDefinition.Anchor.UpperRight: anchorX = texWidthI; anchorY = 0; break;

                    case tk2dSpriteCollectionDefinition.Anchor.Custom: anchorX = thisTexParam.anchorX * gen.globalTextureRescale; anchorY = thisTexParam.anchorY * gen.globalTextureRescale; break;
                }
                Vector3 pos0 = new Vector3(-anchorX * thisTexParam.scale.x * scale, 0, -(h - anchorY * thisTexParam.scale.y) * scale);
				
				colliderOrigin = new Vector3(pos0.x, pos0.z, 0.0f);
                Vector3 pos1 = pos0 + new Vector3(scaleX, 0, scaleY);
				
				untrimmedPos0 = new Vector3(pos0.x, pos0.z);
				untrimmedPos1 = new Vector3(pos1.x, pos1.z);

				List<Vector3> positions = new List<Vector3>();
				List<Vector2> uvs = new List<Vector2>();

				// build mesh
				if (_lut != null && _lut.isSplit)
				{
					coll.spriteDefinitions[i].flipped = tk2dSpriteDefinition.FlipMode.None; // each split could be rotated, but not consistently
					
					for (int j = 0; j < spriteLuts.Count; ++j)
					{
						if (spriteLuts[j].source == i)
						{
							_lut = spriteLuts[j];

							int thisAtlasIndex = 0;
							foreach (var p in packers)
							{
								if ((atlasEntry = p.FindEntryWithIndex(_lut.atlasIndex)) != null)
								{
									packer = p;
									break;
								}
								++thisAtlasIndex;
							}

							if (thisAtlasIndex != atlasIndex)
							{
								// This is a serious problem, dicing is not supported when multi atlas output is selected
								Debug.Break();
							}

							fwidth = packer.width;
				    	    fheight = packer.height;

				            tx = atlasEntry.x + padAmount;
							ty = atlasEntry.y + padAmount;
							tw = atlasEntry.w - padAmount * 2;
							th = atlasEntry.h - padAmount * 2;

				            sd_y = packer.height - ty - th;
				            v0 = new Vector2(tx / fwidth + uvOffsetX, 1.0f - (sd_y + th) / fheight + uvOffsetY);
				            v1 = new Vector2((tx + tw) / fwidth - uvOffsetX, 1.0f - sd_y / fheight - uvOffsetY);

							float x0 = _lut.rx / texWidth;
							float y0 = _lut.ry / texHeight;
							float x1 = (_lut.rx + _lut.rw) / texWidth;
							float y1 = (_lut.ry + _lut.rh) / texHeight;

							Vector3 dpos0 = new Vector3(Mathf.Lerp(pos0.x, pos1.x, x0), 0.0f, Mathf.Lerp(pos0.z, pos1.z, y0));
							Vector3 dpos1 = new Vector3(Mathf.Lerp(pos0.x, pos1.x, x1), 0.0f, Mathf.Lerp(pos0.z, pos1.z, y1));

							positions.Add(new Vector3(dpos0.x, dpos0.z, 0));
							positions.Add(new Vector3(dpos1.x, dpos0.z, 0));
							positions.Add(new Vector3(dpos0.x, dpos1.z, 0));
							positions.Add(new Vector3(dpos1.x, dpos1.z, 0));

			                if (atlasEntry.flipped)
			                {
			                    uvs.Add(new Vector2(v0.x,v0.y));
			                    uvs.Add(new Vector2(v0.x,v1.y));
			                    uvs.Add(new Vector2(v1.x,v0.y));
			                    uvs.Add(new Vector2(v1.x,v1.y));
			                }
			                else
			                {
			                    uvs.Add(new Vector2(v0.x,v0.y));
			                    uvs.Add(new Vector2(v1.x,v0.y));
			                    uvs.Add(new Vector2(v0.x,v1.y));
			                    uvs.Add(new Vector2(v1.x,v1.y));
			                }
						}
					}
				}
				else if (thisTexParam.customSpriteGeometry)
				{
					coll.spriteDefinitions[i].flipped = atlasEntry.flipped ? tk2dSpriteDefinition.FlipMode.Tk2d : tk2dSpriteDefinition.FlipMode.None;
					
					List<int> indices = new List<int>();
					foreach (var island in thisTexParam.geometryIslands)
					{
						int baseIndex = positions.Count;
						for (int x = 0; x < island.points.Length; ++x)
						{
							var v = island.points[x] * gen.globalTextureRescale;
							Vector2 origin = new Vector2(pos0.x, pos0.z);
							positions.Add(new Vector2(v.x * thisTexParam.scale.x, (texHeight - v.y) * thisTexParam.scale.y) * scale + new Vector2(origin.x, origin.y));
							
				            tx = atlasEntry.x + padAmount;
							ty = atlasEntry.y + padAmount;
							tw = atlasEntry.w - padAmount * 2;
							th = atlasEntry.h - padAmount * 2;

				            //v0 = new Vector2(tx / fwidth + uvOffsetX, 1.0f - (sd_y + th) / fheight + uvOffsetY);
				            //v1 = new Vector2((tx + tw) / fwidth - uvOffsetX, 1.0f - sd_y / fheight - uvOffsetY);
							
							Vector2 uv = new Vector2();
							if (atlasEntry.flipped)
							{
								uv.x = (tx - _lut.ry + texHeight - v.y) / fwidth + uvOffsetX;
								uv.y = (ty - _lut.rx + v.x) / fheight + uvOffsetY;
							}
							else
							{
								uv.x = (tx - _lut.rx + v.x) / fwidth + uvOffsetX;
								uv.y = (ty - _lut.ry + texHeight - v.y) / fheight + uvOffsetY ;
							}
							
							uvs.Add(uv);
						}
						
						tk2dEditor.Triangulator triangulator = new tk2dEditor.Triangulator(island.points);
						int[] localIndices = triangulator.Triangulate();
						//for (int x = localIndices.Length - 1; x >= 0; --x)
						for (int x = 0; x < localIndices.Length; x += 3)
						{
							indices.Add(baseIndex + localIndices[x + 2]);
							indices.Add(baseIndex + localIndices[x + 1]);
							indices.Add(baseIndex + localIndices[x + 0]);
						}
					}

					coll.spriteDefinitions[i].indices = indices.ToArray();
				}
				else
				{
					bool flipped = (atlasEntry != null && atlasEntry.flipped);
					coll.spriteDefinitions[i].flipped = flipped ? tk2dSpriteDefinition.FlipMode.Tk2d : tk2dSpriteDefinition.FlipMode.None;
					
					float x0 = 0, y0 = 0;
					float x1 = 0, y1 = 0;

					if (_lut != null) {
						x0 = _lut.rx / texWidth;
						y0 = _lut.ry / texHeight;
						x1 = (_lut.rx + _lut.rw) / texWidth;
						y1 = (_lut.ry + _lut.rh) / texHeight;
					}

					Vector3 dpos0 = new Vector3(Mathf.Lerp(pos0.x, pos1.x, x0), 0.0f, Mathf.Lerp(pos0.z, pos1.z, y0));
					Vector3 dpos1 = new Vector3(Mathf.Lerp(pos0.x, pos1.x, x1), 0.0f, Mathf.Lerp(pos0.z, pos1.z, y1));

					positions.Add(new Vector3(dpos0.x, dpos0.z, 0));
					positions.Add(new Vector3(dpos1.x, dpos0.z, 0));
					positions.Add(new Vector3(dpos0.x, dpos1.z, 0));
					positions.Add(new Vector3(dpos1.x, dpos1.z, 0));

	                if (flipped)
	                {
	                    uvs.Add(new Vector2(v0.x,v0.y));
	                    uvs.Add(new Vector2(v0.x,v1.y));
	                    uvs.Add(new Vector2(v1.x,v0.y));
	                    uvs.Add(new Vector2(v1.x,v1.y));
	                }
	                else
	                {
	                    uvs.Add(new Vector2(v0.x,v0.y));
	                    uvs.Add(new Vector2(v1.x,v0.y));
	                    uvs.Add(new Vector2(v0.x,v1.y));
	                    uvs.Add(new Vector2(v1.x,v1.y));
	                }

					if (thisTexParam.doubleSidedSprite)
					{
						positions.Add(positions[3]); uvs.Add(uvs[3]);
						positions.Add(positions[1]); uvs.Add(uvs[1]);
						positions.Add(positions[2]); uvs.Add(uvs[2]);
						positions.Add(positions[0]); uvs.Add(uvs[0]);
	                }
				}

				// build sprite definition
				if (!thisTexParam.customSpriteGeometry)
				{
					coll.spriteDefinitions[i].indices = new int[ 6 * (positions.Count / 4) ];
					for (int j = 0; j < positions.Count / 4; ++j)
					{
		                coll.spriteDefinitions[i].indices[j * 6 + 0] = j * 4 + 0;
						coll.spriteDefinitions[i].indices[j * 6 + 1] = j * 4 + 3;
						coll.spriteDefinitions[i].indices[j * 6 + 2] = j * 4 + 1;
						coll.spriteDefinitions[i].indices[j * 6 + 3] = j * 4 + 2;
						coll.spriteDefinitions[i].indices[j * 6 + 4] = j * 4 + 3;
						coll.spriteDefinitions[i].indices[j * 6 + 5] = j * 4 + 0;
					}
					coll.spriteDefinitions[i].complexGeometry = false;
				}
				else
				{
					coll.spriteDefinitions[i].complexGeometry = true;
				}
				
				// This doesn't seem to be necessary in UNITY_3_5_3
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5_0 || UNITY_3_5_1 || UNITY_3_5_2)
				if (positions.Count > 0)
				{
					// http://forum.unity3d.com/threads/98781-Compute-mesh-inertia-tensor-failed-for-one-of-the-actor-Behaves-differently-in-3.4
					Vector3 p = positions[positions.Count - 1];
					p.z -= 0.001f;
					positions[positions.Count - 1] = p;
				}
#endif
			
				coll.spriteDefinitions[i].positions = new Vector3[positions.Count];
				coll.spriteDefinitions[i].uvs = new Vector2[uvs.Count];
				for (int j = 0; j < positions.Count; ++j)
				{
					coll.spriteDefinitions[i].positions[j] = positions[j];
					coll.spriteDefinitions[i].uvs[j] = uvs[j];
				}
				
				// empty out to a sensible default, which corresponds to what Unity does by default
				coll.spriteDefinitions[i].normals = new Vector3[0];
				coll.spriteDefinitions[i].tangents = new Vector4[0];
				
				// fill out tangents and normals
				if (gen.normalGenerationMode != tk2dSpriteCollection.NormalGenerationMode.None)
				{
					Mesh tmpMesh = new Mesh();
					tmpMesh.vertices = coll.spriteDefinitions[i].positions;
					tmpMesh.uv = coll.spriteDefinitions[i].uvs;
					tmpMesh.triangles = coll.spriteDefinitions[i].indices;
					
					tmpMesh.RecalculateNormals();
					
					coll.spriteDefinitions[i].normals = tmpMesh.normals;

					if (gen.normalGenerationMode == tk2dSpriteCollection.NormalGenerationMode.NormalsAndTangents)
					{
						Vector4[] tangents = new Vector4[tmpMesh.normals.Length];
						for (int t = 0; t < tangents.Length; ++t)
							tangents[t] = new Vector4(1, 0, 0, 1);
						coll.spriteDefinitions[i].tangents = tangents;
					}
				}
			}
			
			// fixup in case something went wrong
			if (coll.allowMultipleAtlases)
			{
				coll.spriteDefinitions[i].material = gen.atlasMaterials[atlasIndex];
				coll.spriteDefinitions[i].materialId = atlasIndex;
			}
			else
			{
				coll.spriteDefinitions[i].material = gen.altMaterials[thisTexParam.materialId];
				coll.spriteDefinitions[i].materialId = thisTexParam.materialId;
				
				if (coll.spriteDefinitions[i].material == null) // fall back gracefully in case something went wrong
				{
					coll.spriteDefinitions[i].material = gen.atlasMaterials[atlasIndex];
					coll.spriteDefinitions[i].materialId = 0;
				}
			}

            Vector3 boundsMin = new Vector3(1.0e32f, 1.0e32f, 1.0e32f);
            Vector3 boundsMax = new Vector3(-1.0e32f, -1.0e32f, -1.0e32f);
            foreach (Vector3 v in coll.spriteDefinitions[i].positions)
            {
                boundsMin = Vector3.Min(boundsMin, v);
                boundsMax = Vector3.Max(boundsMax, v);
            }
			
			coll.spriteDefinitions[i].boundsData = new Vector3[2];
			coll.spriteDefinitions[i].boundsData[0] = (boundsMax + boundsMin) / 2.0f;
			coll.spriteDefinitions[i].boundsData[1] = (boundsMax - boundsMin);

			// this is the dimension of exactly one pixel, scaled to match sprite dimensions and scale
			coll.spriteDefinitions[i].texelSize = new Vector3(scale * thisTexParam.scale.x / gen.globalScale, scale * thisTexParam.scale.y / gen.globalScale, 0.0f);
			
			coll.spriteDefinitions[i].untrimmedBoundsData = new Vector3[2];
			if (mesh)
			{
				// custom meshes aren't trimmed, the untrimmed bounds are exactly the same as the regular ones
				coll.spriteDefinitions[i].untrimmedBoundsData[0] = coll.spriteDefinitions[i].boundsData[0];
				coll.spriteDefinitions[i].untrimmedBoundsData[1] = coll.spriteDefinitions[i].boundsData[1];
			}
			else
			{
				boundsMin = Vector3.Min(untrimmedPos0, untrimmedPos1);
				boundsMax = Vector3.Max(untrimmedPos0, untrimmedPos1);
				coll.spriteDefinitions[i].untrimmedBoundsData[0] = (boundsMax + boundsMin) / 2.0f;
				coll.spriteDefinitions[i].untrimmedBoundsData[1] = (boundsMax - boundsMin);
			}
			
			
			coll.spriteDefinitions[i].name = gen.textureParams[i].name;

			// Generate collider data here
			UpdateColliderData(gen, scale * gen.globalTextureRescale, coll, i, colliderOrigin);

			// Generate attach point data here
			UpdateAttachPointData(gen, scale * gen.globalTextureRescale, coll, i, colliderOrigin);
        }
    }
	
    static void UpdateAttachPointData(tk2dSpriteCollection gen, float scale, tk2dSpriteCollectionData target, int spriteId, Vector3 origin) {
		tk2dSpriteCollectionDefinition src = gen.textureParams[spriteId];
		tk2dSpriteDefinition def = target.spriteDefinitions[spriteId];
		float texHeight = 0;
		if (src.extractRegion) {
			texHeight = src.regionH;
		}
		else {
			texHeight = gen.textureParams[spriteId].texture?gen.textureParams[spriteId].texture.height:2.0f;
		}


		def.attachPoints = new tk2dSpriteDefinition.AttachPoint[ src.attachPoints.Count ];
		for (int i = 0; i < src.attachPoints.Count; ++i) {
			tk2dSpriteDefinition.AttachPoint srcP = src.attachPoints[i];
			tk2dSpriteDefinition.AttachPoint p = new tk2dSpriteDefinition.AttachPoint();
			p.CopyFrom( src.attachPoints[i] );
			// Rescale position to be in sprite local space
			p.position = new Vector2(srcP.position.x * src.scale.x, (texHeight - srcP.position.y) * src.scale.y) * scale + new Vector2(origin.x, origin.y);
			def.attachPoints[i] = p;
		}
    }

	static void UpdateColliderData(tk2dSpriteCollection gen, float scale, tk2dSpriteCollectionData coll, int spriteIndex, Vector3 origin)
	{
		var colliderType = gen.textureParams[spriteIndex].colliderType;
		var def = coll.spriteDefinitions[spriteIndex];
		var src = gen.textureParams[spriteIndex];
		
		def.colliderVertices = null;
		def.colliderIndicesFwd = null;
		def.colliderIndicesBack = null;
		
		float texHeight = 0;
		if (src.extractRegion)
		{
			texHeight = src.regionH;
		}
		else
		{
			texHeight = gen.textureParams[spriteIndex].texture?gen.textureParams[spriteIndex].texture.height:2.0f;
		}
		
		if (colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxTrimmed)
		{
			def.colliderVertices = new Vector3[2];
			def.colliderVertices[0] = def.boundsData[0];
			def.colliderVertices[1] = def.boundsData[1] * 0.5f; // extents is 1/2x size
			def.colliderVertices[1].z = gen.physicsDepth;
			def.colliderType = tk2dSpriteDefinition.ColliderType.Box;
		}
		else if (colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom)
		{
			Vector2 v0 = new Vector3(src.boxColliderMin.x * src.scale.x, (texHeight - src.boxColliderMax.y) * src.scale.y) * scale + origin;
			Vector2 v1 = new Vector3(src.boxColliderMax.x * src.scale.x, (texHeight - src.boxColliderMin.y) * src.scale.y) * scale + origin;
			
			def.colliderVertices = new Vector3[2];
			def.colliderVertices[0] = (v0 + v1) * 0.5f;
			def.colliderVertices[1] = (v1 - v0) * 0.5f;
			def.colliderVertices[1].z = gen.physicsDepth;
			def.colliderType = tk2dSpriteDefinition.ColliderType.Box;
		}
		else if (colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
		{
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> meshIndicesFwd = new List<int>();
			
			foreach (var island in src.polyColliderIslands)
			{
				List<Vector2> points = new List<Vector2>();
				List<Vector2> points2D = new List<Vector2>();
				
				// List all points
				for (int i = 0; i < island.points.Length; ++i)
				{
					Vector2 v = island.points[i];
					points.Add(new Vector2(v.x * src.scale.x, (texHeight - v.y) * src.scale.y) * scale + new Vector2(origin.x, origin.y));
				}
				
				int baseIndex = meshVertices.Count;
				for (int i = 0; i < points.Count; ++i)
				{
					meshVertices.Add( new Vector3(points[i].x, points[i].y, -gen.physicsDepth) );
					meshVertices.Add( new Vector3(points[i].x, points[i].y,  gen.physicsDepth) );
					points2D.Add( new Vector2(points[i].x, points[i].y) );
				}
				
				// body
				int numPoints = island.connected?points.Count:(points.Count - 1);
				for (int i = 0; i < numPoints; ++i)
				{
					int i0 = i * 2;
					int i1 = i0 + 1;
					int i2 = ((i + 1)%island.points.Length) * 2;
					int i3 = i2 + 1;
					
					// Classify primary edge direction, and flip accordingly
					bool flipIndices = false;
					Vector2 grad = points2D[i] - points2D[(i + 1) % points2D.Count];
					if (Mathf.Abs(grad.x) < Mathf.Abs(grad.y))
					{
						flipIndices = (grad.y > 0.0f);
					}
					else
					{
						flipIndices = (grad.x > 0.0f);
					}
					
					if (flipIndices)
					{
						meshIndicesFwd.Add(baseIndex + i2);
						meshIndicesFwd.Add(baseIndex + i3);
						meshIndicesFwd.Add(baseIndex + i0);
						meshIndicesFwd.Add(baseIndex + i0);
						meshIndicesFwd.Add(baseIndex + i3);
						meshIndicesFwd.Add(baseIndex + i1);
					}
					else
					{
						meshIndicesFwd.Add(baseIndex + i2);
						meshIndicesFwd.Add(baseIndex + i1);
						meshIndicesFwd.Add(baseIndex + i0);
						meshIndicesFwd.Add(baseIndex + i2);
						meshIndicesFwd.Add(baseIndex + i3);
						meshIndicesFwd.Add(baseIndex + i1);
					}
				}

				// cap if allowed and necessary
				var cap = src.polyColliderCap;
				if (island.connected && cap != tk2dSpriteCollectionDefinition.PolygonColliderCap.None)
				{
					tk2dEditor.Triangulator triangulator = new tk2dEditor.Triangulator(points2D.ToArray());
					int[] indices = triangulator.Triangulate();
					
					if (cap == tk2dSpriteCollectionDefinition.PolygonColliderCap.Front || cap == tk2dSpriteCollectionDefinition.PolygonColliderCap.FrontAndBack)
					{
						for (int i = 0; i < indices.Length; ++i)
							meshIndicesFwd.Add(baseIndex + indices[i] * 2);
					}
					
					if (cap == tk2dSpriteCollectionDefinition.PolygonColliderCap.Back || cap == tk2dSpriteCollectionDefinition.PolygonColliderCap.FrontAndBack)
					{
						for (int i = 0; i < indices.Length; i += 3)
						{
							meshIndicesFwd.Add(baseIndex + indices[i + 2] * 2 + 1);
							meshIndicesFwd.Add(baseIndex + indices[i + 1] * 2 + 1);
							meshIndicesFwd.Add(baseIndex + indices[i + 0] * 2 + 1);
						}
					}
				}
			}
			
			int[] meshIndicesBack = new int[meshIndicesFwd.Count];
			for (int i = 0; i < meshIndicesFwd.Count; i += 3)
			{
				meshIndicesBack[i + 0] = meshIndicesFwd[i + 2];
				meshIndicesBack[i + 1] = meshIndicesFwd[i + 1];
				meshIndicesBack[i + 2] = meshIndicesFwd[i + 0];
			}
			
			def.colliderVertices = meshVertices.ToArray();
			def.colliderIndicesFwd = meshIndicesFwd.ToArray();
			def.colliderIndicesBack = meshIndicesBack;
			def.colliderConvex = src.colliderConvex;
			def.colliderType = tk2dSpriteDefinition.ColliderType.Mesh;
			def.colliderSmoothSphereCollisions = src.colliderSmoothSphereCollisions;
		}
		else if (colliderType == tk2dSpriteCollectionDefinition.ColliderType.ForceNone)
		{
			def.colliderType = tk2dSpriteDefinition.ColliderType.None;
		}
		else if (colliderType == tk2dSpriteCollectionDefinition.ColliderType.UserDefined)
		{
			def.colliderType = tk2dSpriteDefinition.ColliderType.Unset;
		}
	}
}
