using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	// As nasty as this is, its a necessary evil for backwards compatibility
	public class SpriteCollectionProxy
	{
		public SpriteCollectionProxy()
		{
		}
		
		public SpriteCollectionProxy(tk2dSpriteCollection obj)
		{
			this.obj = obj;
			CopyFromSource();
		}
		
		public void CopyFromSource()
		{
			this.obj.Upgrade(); // make sure its up to date

			textureParams = new List<tk2dSpriteCollectionDefinition>(obj.textureParams.Length);
			foreach (var v in obj.textureParams)
			{
				if (v == null) 
					textureParams.Add(null);
				else 
				{
					var t = new tk2dSpriteCollectionDefinition();
					t.CopyFrom(v);
					textureParams.Add(t);
				}
			}
			
			spriteSheets = new List<tk2dSpriteSheetSource>();
			if (obj.spriteSheets != null)
			{
				foreach (var v in obj.spriteSheets)
				{
					if (v == null) 
						spriteSheets.Add(null);
					else
					{
						var t = new tk2dSpriteSheetSource();
						t.CopyFrom(v);
						spriteSheets.Add(t);
					}
				}
			}
			
			fonts = new List<tk2dSpriteCollectionFont>();
			if (obj.fonts != null)
			{
				foreach (var v in obj.fonts)
				{
					if (v == null)
						fonts.Add(null);
					else
					{
						var t = new tk2dSpriteCollectionFont();
						t.CopyFrom(v);
						fonts.Add(t);
					}
				}
			}

			attachPointTestSprites.Clear();
			foreach (var v in obj.attachPointTestSprites) {
				if (v.spriteCollection != null && v.spriteCollection.IsValidSpriteId(v.spriteId)) {
					tk2dSpriteCollection.AttachPointTestSprite ap = new tk2dSpriteCollection.AttachPointTestSprite();
					ap.CopyFrom(v);
					attachPointTestSprites[v.attachPointName] = v;
				}
			}

			UpgradeLegacySpriteSheets();
			
			var target = this;
			var source = obj;
			
			target.platforms = new List<tk2dSpriteCollectionPlatform>();
			foreach (tk2dSpriteCollectionPlatform plat in source.platforms)
			{
				tk2dSpriteCollectionPlatform p = new tk2dSpriteCollectionPlatform();
				p.CopyFrom(plat);
				target.platforms.Add(p);
			}
			if (target.platforms.Count == 0)
			{
				tk2dSpriteCollectionPlatform plat = new tk2dSpriteCollectionPlatform(); // add a null platform
				target.platforms.Add(plat);
			}

			target.assetName = source.assetName;
			target.loadable = source.loadable;

			target.maxTextureSize = source.maxTextureSize;
			target.forceTextureSize = source.forceTextureSize;
			target.forcedTextureWidth = source.forcedTextureWidth;
			target.forcedTextureHeight = source.forcedTextureHeight;
			
			target.textureCompression = source.textureCompression;
			target.atlasWidth = source.atlasWidth;
			target.atlasHeight = source.atlasHeight;
			target.forceSquareAtlas = source.forceSquareAtlas;
			target.atlasWastage = source.atlasWastage;
			target.allowMultipleAtlases = source.allowMultipleAtlases;
			target.removeDuplicates = source.removeDuplicates;
			
			target.spriteCollection = source.spriteCollection;
			target.premultipliedAlpha = source.premultipliedAlpha;
			
			CopyArray(ref target.altMaterials, source.altMaterials);
			CopyArray(ref target.atlasMaterials, source.atlasMaterials);
			CopyArray(ref target.atlasTextures, source.atlasTextures);
			
			target.sizeDef.CopyFrom( source.sizeDef );
			target.globalScale = source.globalScale;
			target.globalTextureRescale = source.globalTextureRescale;
			target.physicsDepth = source.physicsDepth;
			target.disableTrimming = source.disableTrimming;
			target.normalGenerationMode = source.normalGenerationMode;
			target.padAmount = source.padAmount;
			target.autoUpdate = source.autoUpdate;
			target.editorDisplayScale = source.editorDisplayScale;

			// Texture settings
			target.filterMode = source.filterMode;
			target.wrapMode = source.wrapMode;
			target.userDefinedTextureSettings = source.userDefinedTextureSettings;
			target.mipmapEnabled = source.mipmapEnabled;
			target.anisoLevel = source.anisoLevel;
		}
		
		void CopyArray<T>(ref T[] dest, T[] source)
		{
			if (source == null)
			{
				dest = new T[0];
			}
			else
			{
				dest = new T[source.Length];
				for (int i = 0; i < source.Length; ++i)
					dest[i] = source[i];
			}
		}
		
		void UpgradeLegacySpriteSheets()
		{
			if (spriteSheets != null)
			{
				for (int i = 0; i < spriteSheets.Count; ++i)
				{
					var spriteSheet = spriteSheets[i];
					if (spriteSheet != null && spriteSheet.version == 0)
					{
						if (spriteSheet.texture == null)
						{
							spriteSheet.active = false;
						}
						else
						{
							spriteSheet.tileWidth = spriteSheet.texture.width / spriteSheet.tilesX;
							spriteSheet.tileHeight = spriteSheet.texture.height / spriteSheet.tilesY;
							spriteSheet.active = true;
							
							for (int j = 0; j < textureParams.Count; ++j)
							{
								var param = textureParams[j];
								if (param.fromSpriteSheet && param.texture == spriteSheet.texture)
								{
									param.fromSpriteSheet = false;
									param.hasSpriteSheetId = true;
									param.spriteSheetId = i;
									
									param.spriteSheetX = param.regionId % spriteSheet.tilesX;
									param.spriteSheetY = param.regionId / spriteSheet.tilesX;
								}
							}
						}
						
						spriteSheet.version = tk2dSpriteSheetSource.CURRENT_VERSION;
					}
				}				
			}
		}

		public void DeleteUnusedData()
		{
			foreach (tk2dSpriteCollectionFont font in obj.fonts)
			{
				bool found = false;
				foreach (tk2dSpriteCollectionFont f in fonts)
				{
					if (f.data == font.data && f.editorData == font.editorData)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					tk2dEditorUtility.DeleteAsset(font.data);
					tk2dEditorUtility.DeleteAsset(font.editorData);
				}
			}

			if (obj.altMaterials != null)
			{
				foreach (Material material in obj.altMaterials)
				{
					bool found = false;
					if (altMaterials != null)
					{
						foreach (Material m in altMaterials)
						{
							if (m == material)
							{
								found = true;
								break;
							}
						}
					}
					if (!found)
						tk2dEditorUtility.DeleteAsset(material);
				}
			}

			List<tk2dSpriteCollectionPlatform> platformsToDelete = new List<tk2dSpriteCollectionPlatform>();
			if (obj.HasPlatformData && !this.HasPlatformData)
			{
				platformsToDelete = new List<tk2dSpriteCollectionPlatform>(obj.platforms);
				atlasTextures = new Texture2D[0]; // clear all references
				atlasMaterials = new Material[0];
			}
			else if (this.HasPlatformData && !obj.HasPlatformData)
			{
				// delete old sprite collection atlases and materials
				foreach (Material material in obj.atlasMaterials)
					tk2dEditorUtility.DeleteAsset(material);
				foreach (Texture2D texture in obj.atlasTextures)
					tk2dEditorUtility.DeleteAsset(texture);
			}
			else if (obj.HasPlatformData && this.HasPlatformData)
			{
				foreach (tk2dSpriteCollectionPlatform platform in obj.platforms)
				{
					bool found = false;
					foreach (tk2dSpriteCollectionPlatform p in platforms)
					{
						if (p.spriteCollection == platform.spriteCollection)
						{
							found = true;
							break;
						}
					}
					if (!found) // platform existed previously, but does not any more
						platformsToDelete.Add(platform);
				}
			}

			foreach (tk2dSpriteCollectionPlatform platform in platformsToDelete)
			{
				if (platform.spriteCollection == null) continue;
				tk2dSpriteCollection sc = platform.spriteCollection;
				string path = AssetDatabase.GetAssetPath(sc.spriteCollection);

				tk2dEditorUtility.DeleteAsset(sc.spriteCollection);
				foreach (Material material in sc.atlasMaterials)
					tk2dEditorUtility.DeleteAsset(material);
				foreach (Texture2D texture in sc.atlasTextures)
					tk2dEditorUtility.DeleteAsset(texture);
				foreach (tk2dSpriteCollectionFont font in sc.fonts)
				{
					tk2dEditorUtility.DeleteAsset(font.editorData);
					tk2dEditorUtility.DeleteAsset(font.data);
				}

				tk2dEditorUtility.DeleteAsset(sc);

				string dataDirName = System.IO.Path.GetDirectoryName(path);
				if (System.IO.Directory.Exists(dataDirName) && System.IO.Directory.GetFiles(dataDirName).Length == 0)
					AssetDatabase.DeleteAsset(dataDirName);
			}
		}

		public void ClearReferences() {
			altMaterials = new Material[0];
			atlasMaterials = new Material[0];
			atlasTextures = new Texture2D[0];
			spriteCollection = null;

			obj.altMaterials = new Material[0];
			obj.atlasMaterials = new Material[0];
			obj.atlasTextures = new Texture2D[0];
			obj.spriteCollection = null;
		}

		public void CopyToTarget()
		{
			CopyToTarget(obj);
		}
		
		public void CopyToTarget(tk2dSpriteCollection target)
		{
			target.textureParams = textureParams.ToArray();
			target.spriteSheets = spriteSheets.ToArray();
			target.fonts = fonts.ToArray();

			var source = this;
			target.platforms = new List<tk2dSpriteCollectionPlatform>();
			foreach (tk2dSpriteCollectionPlatform plat in source.platforms)
			{
				tk2dSpriteCollectionPlatform p = new tk2dSpriteCollectionPlatform();
				p.CopyFrom(plat);
				target.platforms.Add(p);
			}
			target.assetName = source.assetName;
			target.loadable = source.loadable;
			
			target.maxTextureSize = source.maxTextureSize;
			target.forceTextureSize = source.forceTextureSize;
			target.forcedTextureWidth = source.forcedTextureWidth;
			target.forcedTextureHeight = source.forcedTextureHeight;
			
			target.textureCompression = source.textureCompression;
			target.atlasWidth = source.atlasWidth;
			target.atlasHeight = source.atlasHeight;
			target.forceSquareAtlas = source.forceSquareAtlas;
			target.atlasWastage = source.atlasWastage;
			target.allowMultipleAtlases = source.allowMultipleAtlases;
			target.removeDuplicates = source.removeDuplicates;
			
			target.spriteCollection = source.spriteCollection;
			target.premultipliedAlpha = source.premultipliedAlpha;
			
			CopyArray(ref target.altMaterials, source.altMaterials);
			CopyArray(ref target.atlasMaterials, source.atlasMaterials);
			CopyArray(ref target.atlasTextures, source.atlasTextures);

			target.sizeDef.CopyFrom( source.sizeDef );
			target.globalScale = source.globalScale;
			target.globalTextureRescale = source.globalTextureRescale;
			target.physicsDepth = source.physicsDepth;
			target.disableTrimming = source.disableTrimming;
			target.normalGenerationMode = source.normalGenerationMode;
			target.padAmount = source.padAmount; 
			target.autoUpdate = source.autoUpdate;
			target.editorDisplayScale = source.editorDisplayScale;

			// Texture settings
			target.filterMode = source.filterMode;
			target.wrapMode = source.wrapMode;
			target.userDefinedTextureSettings = source.userDefinedTextureSettings;
			target.mipmapEnabled = source.mipmapEnabled;
			target.anisoLevel = source.anisoLevel;

			// Attach point test data
			// Make sure we only store relevant data
			HashSet<string> attachPointNames = new HashSet<string>();
			foreach (tk2dSpriteCollectionDefinition def in textureParams) {
				foreach (tk2dSpriteDefinition.AttachPoint ap in def.attachPoints) {
					attachPointNames.Add(ap.name);
				}
			}
			target.attachPointTestSprites.Clear();
			foreach (string name in attachPointTestSprites.Keys) {
				if (attachPointNames.Contains(name)) {
					tk2dSpriteCollection.AttachPointTestSprite lut = new tk2dSpriteCollection.AttachPointTestSprite();
					lut.CopyFrom( attachPointTestSprites[name] );
					lut.attachPointName = name;
					target.attachPointTestSprites.Add(lut);
				}
			}
		}
		
		public bool AllowAltMaterials
		{
			get
			{
				return !allowMultipleAtlases;
			}
		}
		
		public int FindOrCreateEmptySpriteSlot()
		{
			for (int index = 0; index < textureParams.Count; ++index)
			{
				if (textureParams[index].texture == null && textureParams[index].name.Length == 0)
					return index;
			}
			textureParams.Add(new tk2dSpriteCollectionDefinition());
			return textureParams.Count - 1;
		}
		
		public int FindOrCreateEmptyFontSlot()
		{
			for (int index = 0; index < fonts.Count; ++index)
			{
				if (!fonts[index].active)
				{
					fonts[index].active = true;
					return index;
				}
			}
			var font = new tk2dSpriteCollectionFont();
			font.active = true;
			fonts.Add(font);
			return fonts.Count - 1;
		}
		
		public int FindOrCreateEmptySpriteSheetSlot()
		{
			for (int index = 0; index < spriteSheets.Count; ++index)
			{
				if (!spriteSheets[index].active)
				{
					spriteSheets[index].active = true;
					spriteSheets[index].version = tk2dSpriteSheetSource.CURRENT_VERSION;
					return index;
				}
			}
			var spriteSheet = new tk2dSpriteSheetSource();
			spriteSheet.active = true;
			spriteSheet.version = tk2dSpriteSheetSource.CURRENT_VERSION;
			spriteSheets.Add(spriteSheet);
			return spriteSheets.Count - 1;
		}
		
		public string FindUniqueTextureName(string name)
		{
			int at = name.LastIndexOf('@');
			if (at != -1) {
				name = name.Substring(0, at);
			}

			List<string> textureNames = new List<string>();
			foreach (var entry in textureParams)
			{
				textureNames.Add(entry.name);
			}
			if (textureNames.IndexOf(name) == -1)
				return name;
			int count = 1;
			do 
			{
				string currName = name + " " + count.ToString();
				if (textureNames.IndexOf(currName) == -1)
					return currName;
				++count;
			} while(count < 1024); // arbitrary large number
			return name; // failed to find a name
		}

		public int FindSpriteBySource(Texture2D tex) {
			for (int i = 0; i < textureParams.Count; ++i) {
				if (textureParams[i].texture == tex) {
					return i;
				}
			}
			return -1;
		}
		
		public bool Empty { get { return textureParams.Count == 0 && fonts.Count == 0 && spriteSheets.Count == 0; } }
		
		// Call after deleting anything
		public void Trim()
		{
			int lastIndex = textureParams.Count - 1;
			while (lastIndex >= 0)
			{
				if (textureParams[lastIndex].texture != null || textureParams[lastIndex].name.Length > 0)
					break;
				lastIndex--;
			}
			int count = textureParams.Count - 1 - lastIndex;
			if (count > 0)
			{
				textureParams.RemoveRange( lastIndex + 1, count );
			}
			
			lastIndex = fonts.Count - 1;
			while (lastIndex >= 0)
			{
				if (fonts[lastIndex].active)
					break;
				lastIndex--;
			}
			count = fonts.Count - 1 - lastIndex;
			if (count > 0) fonts.RemoveRange(lastIndex + 1, count);
			
			lastIndex = spriteSheets.Count - 1;
			while (lastIndex >= 0)
			{
				if (spriteSheets[lastIndex].active)
					break;
				lastIndex--;
			}
			count = spriteSheets.Count - 1 - lastIndex;
			if (count > 0) spriteSheets.RemoveRange(lastIndex + 1, count);
			
			lastIndex = atlasMaterials.Length - 1;
			while (lastIndex >= 0)
			{
				if (atlasMaterials[lastIndex] != null)
					break;
				lastIndex--;
			}
			count = atlasMaterials.Length - 1 - lastIndex;
			if (count > 0) 
				System.Array.Resize(ref atlasMaterials, lastIndex + 1);
		}
		
		public int GetSpriteSheetId(tk2dSpriteSheetSource spriteSheet)
		{
			for (int index = 0; index < spriteSheets.Count; ++index)
				if (spriteSheets[index] == spriteSheet) return index;
			return 0;
		}
		
		// Delete all sprites from a spritesheet
		public void DeleteSpriteSheet(tk2dSpriteSheetSource spriteSheet)
		{
			int index = GetSpriteSheetId(spriteSheet);
			
			for (int i = 0; i < textureParams.Count; ++i)
			{
				if (textureParams[i].hasSpriteSheetId && textureParams[i].spriteSheetId == index)
				{
					textureParams[i] = new tk2dSpriteCollectionDefinition();
				}
			}
			
			spriteSheets[index] = new tk2dSpriteSheetSource();
			Trim();
		}
		
		public string GetAssetPath()
		{
			return AssetDatabase.GetAssetPath(obj);
		}

		public string GetOrCreateDataPath()
		{
			return tk2dSpriteCollectionBuilder.GetOrCreateDataPath(obj);
		}

		public bool Ready { get { return obj != null; } }
		tk2dSpriteCollection obj;
		

		// Mirrored data objects
		public List<tk2dSpriteCollectionDefinition> textureParams = new List<tk2dSpriteCollectionDefinition>();
		public List<tk2dSpriteSheetSource> spriteSheets = new List<tk2dSpriteSheetSource>();
		public List<tk2dSpriteCollectionFont> fonts = new List<tk2dSpriteCollectionFont>();
		
		// Mirrored from sprite collection
		public string assetName;
		public int maxTextureSize;
		public tk2dSpriteCollection.TextureCompression textureCompression;
		public int atlasWidth, atlasHeight;
		public bool forceSquareAtlas;
		public float atlasWastage;
		public bool allowMultipleAtlases;
		public bool removeDuplicates;
		public tk2dSpriteCollectionData spriteCollection;
	    public bool premultipliedAlpha;

	    public List<tk2dSpriteCollectionPlatform> platforms = new List<tk2dSpriteCollectionPlatform>();
		public bool HasPlatformData { get { return platforms.Count > 1; } }
		
		public Material[] altMaterials;
		public Material[] atlasMaterials;
		public Texture2D[] atlasTextures;
		
		public tk2dSpriteCollectionSize sizeDef = new tk2dSpriteCollectionSize();

		public float globalScale;
		public float globalTextureRescale;

		// Attach point test data
		public Dictionary<string, tk2dSpriteCollection.AttachPointTestSprite> attachPointTestSprites = new Dictionary<string, tk2dSpriteCollection.AttachPointTestSprite>();

		// Texture settings
		public FilterMode filterMode;
		public TextureWrapMode wrapMode;
		public bool userDefinedTextureSettings;
		public bool mipmapEnabled = true;
		public int anisoLevel = 1;

		public float physicsDepth;
		public bool disableTrimming;
		
		public bool forceTextureSize = false;
		public int forcedTextureWidth = 1024;
		public int forcedTextureHeight = 1024;
		
		public tk2dSpriteCollection.NormalGenerationMode normalGenerationMode;
		public int padAmount;
		public bool autoUpdate;
		public bool loadable;
		
		public float editorDisplayScale;
	}
}

