using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace tk2dEditor.SpriteCollectionBuilder
{
	public static class PlatformBuilder
	{
		public static void InitializeSpriteCollectionPlatforms(tk2dSpriteCollection gen, string root)
		{
			// Create all missing platform directories and sprite collection objects
			for (int i = 0; i < gen.platforms.Count; ++i)
			{
				tk2dSpriteCollectionPlatform plat = gen.platforms[i];
				if (plat.name.Length > 0 && !plat.spriteCollection)
				{
					plat.spriteCollection = tk2dSpriteCollectionEditor.CreateSpriteCollection(root, gen.name + "@" + plat.name);
					plat.spriteCollection.managedSpriteCollection = true;
					EditorUtility.SetDirty(gen.spriteCollection);
				}
			}
		}

		static string FindFileInPath(string directory, string filename, string preferredExt, string[] otherExts)
		{
			string target = directory + "/" + filename + preferredExt;
			if (System.IO.File.Exists(target)) return target;

			foreach (string ext in otherExts)
			{
				if (ext == preferredExt) continue;

				target = directory + "/" + filename + ext;
				if (System.IO.File.Exists(target)) return target;
			}

			return ""; // not found
		}

		static readonly string[] textureExtensions = { ".psd", ".tiff", ".jpg", ".jpeg", ".tga", ".png", ".gif", ".bmp", ".iff", ".pict" };
		static readonly string[] fontExtensions = { ".fnt", ".xml", ".txt" };

		// Given a path to a texture, finds a platform specific version of it. Returns "" if not found in search paths
		static string FindAssetForPlatform(string platformName, string path, string[] extensions)
		{
			string directory = System.IO.Path.GetDirectoryName(path);
			string ext = System.IO.Path.GetExtension(path);

			string filename = System.IO.Path.GetFileNameWithoutExtension(path);
			int lastIndexOf = filename.LastIndexOf('@');
			if (lastIndexOf != -1)
				filename = filename.Substring(0, lastIndexOf);

			// Find texture with same filename @platform

			// The first preferred path is with the filename@platform.ext
			string platformTexture = FindFileInPath(directory, filename + "@" + platformName, ext, extensions);

			// Second path to look for is platform/filename.ext
			if (platformTexture.Length == 0)
			{
				string altDirectory = directory + "/" + platformName;
				if (System.IO.Directory.Exists(altDirectory))
					 platformTexture = FindFileInPath(altDirectory, filename, ext, extensions);
			}

			// Third path to look for is platform/filename@platform.ext
			if (platformTexture.Length == 0)
			{
				string altDirectory = directory + "/" + platformName;
				if (System.IO.Directory.Exists(altDirectory))
					 platformTexture = FindFileInPath(altDirectory, filename + "@" + platformName, ext, extensions);
			}

			// Fourth path to look for is ../platform/filename.ext - so you can have all textures in platform folders
			// Based on a contribution by Marcus Svensson
			if (platformTexture.Length == 0) {
				int lastIndex = directory.LastIndexOf("/"); 
				if (lastIndex >= 0) {
					string parentDirectory = directory.Remove(lastIndex, directory.Length - lastIndex); 
					string altDirectory = parentDirectory + "/" + platformName; 
					if (System.IO.Directory.Exists(altDirectory)) 
						platformTexture = FindFileInPath(altDirectory, filename, ext, extensions); 
				}
			}

			return platformTexture;
		}

		// Update target platforms
		public static void UpdatePlatformSpriteCollection(tk2dSpriteCollection source, tk2dSpriteCollection target, string dataPath, bool root, float scale, string platformName)
		{
			tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy proxy = new tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy(source);
			
			// Restore old sprite collection
			proxy.spriteCollection = target.spriteCollection;
			
			proxy.atlasTextures = target.atlasTextures;
			proxy.atlasMaterials = target.atlasMaterials;
			proxy.altMaterials = target.altMaterials;

			// This must always be zero, as children cannot have nested platforms.
			// That would open the door to a lot of unnecessary insanity
			proxy.platforms = new List<tk2dSpriteCollectionPlatform>();

			// Update atlas sizes
			proxy.atlasWidth = (int)(proxy.atlasWidth * scale);
			proxy.atlasHeight = (int)(proxy.atlasHeight * scale);
			proxy.maxTextureSize = (int)(proxy.maxTextureSize * scale);
			proxy.forcedTextureWidth = (int)(proxy.forcedTextureWidth * scale);
			proxy.forcedTextureHeight = (int)(proxy.forcedTextureHeight * scale);

			proxy.globalScale = 1.0f / scale;

			// Don't bother changing stuff on the root object
			// The root object is the one that the sprite collection is defined on initially
			if (!root)
			{
				// Update textures
				foreach (tk2dSpriteCollectionDefinition param in proxy.textureParams)
				{
					if (param.texture == null) continue;
					
					string path = AssetDatabase.GetAssetPath(param.texture);
					string platformTexture = FindAssetForPlatform(platformName, path, textureExtensions);

					if (platformTexture.Length == 0)
					{
						LogNotFoundError(platformName, param.texture.name, "texture");
					}
					else
					{
						Texture2D tex = AssetDatabase.LoadAssetAtPath(platformTexture, typeof(Texture2D)) as Texture2D;
						if (tex == null)
						{
							Debug.LogError("Unable to load platform specific texture '" + platformTexture + "'");
						}
						else
						{
							param.texture = tex;
						}
					}

					// Handle spritesheets. Odd coordinates could cause issues
					if (param.extractRegion)
					{
						param.regionX = (int)(param.regionX * scale);
						param.regionY = (int)(param.regionY * scale);
						param.regionW = (int)(param.regionW * scale);
						param.regionH = (int)(param.regionH * scale);
					}

					if (param.anchor == tk2dSpriteCollectionDefinition.Anchor.Custom)
					{
						param.anchorX = (int)(param.anchorX * scale);
						param.anchorY = (int)(param.anchorY * scale);
					}

					if (param.customSpriteGeometry)
					{
						foreach (tk2dSpriteColliderIsland geom in param.geometryIslands)
						{
							for (int p = 0; p < geom.points.Length; ++p)
								geom.points[p] *= scale;
						}
					}
					else if (param.dice)
					{
						param.diceUnitX = (int)(param.diceUnitX * scale);
						param.diceUnitY = (int)(param.diceUnitY * scale);
					}

					if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.Polygon)
					{
						foreach (tk2dSpriteColliderIsland geom in param.polyColliderIslands)
						{
							for (int p = 0; p < geom.points.Length; ++p)
								geom.points[p] *= scale;
						}
					}
					else if (param.colliderType == tk2dSpriteCollectionDefinition.ColliderType.BoxCustom)
					{
						param.boxColliderMax *= scale;
						param.boxColliderMin *= scale;
					}

					for (int i = 0; i < param.attachPoints.Count; ++i) {
						param.attachPoints[i].position = param.attachPoints[i].position * scale;
					}
				}
			}

			// We ALWAYS duplicate fonts
			if (target.fonts == null) target.fonts = new tk2dSpriteCollectionFont[0];
			for (int i = 0; i < proxy.fonts.Count; ++i)
			{
				tk2dSpriteCollectionFont font = proxy.fonts[i];
				if (!font.InUse || font.texture == null || font.data == null || font.editorData == null || font.bmFont == null) continue; // not valid for some reason or other
				bool needFontData = true;
				bool needFontEditorData = true;
				bool hasCorrespondingData = i < target.fonts.Length && target.fonts[i] != null;
				if (hasCorrespondingData)
				{
					tk2dSpriteCollectionFont targetFont = target.fonts[i];
					if (targetFont.data != null) { font.data = targetFont.data; needFontData = false; }
					if (targetFont.editorData != null) { font.editorData = targetFont.editorData; needFontEditorData = false; }
				}

				string bmFontPath = AssetDatabase.GetAssetPath(font.bmFont);
				string texturePath = AssetDatabase.GetAssetPath(font.texture);

				if (!root)
				{
					// find platform specific versions 
					bmFontPath = FindAssetForPlatform(platformName, bmFontPath, fontExtensions);
					texturePath = FindAssetForPlatform(platformName, texturePath, textureExtensions);
					if (bmFontPath.Length != 0 && texturePath.Length == 0)
					{
						// try to find a texture
						tk2dEditor.Font.Info fontInfo = tk2dEditor.Font.Builder.ParseBMFont(bmFontPath);
						if (fontInfo != null)
							texturePath = System.IO.Path.GetDirectoryName(bmFontPath).Replace('\\', '/') + "/" + System.IO.Path.GetFileName(fontInfo.texturePaths[0]);
					}

					if (bmFontPath.Length == 0) LogNotFoundError(platformName, font.bmFont.name, "font");
					if (texturePath.Length == 0) LogNotFoundError(platformName, font.texture.name, "texture");
					if (bmFontPath.Length == 0 || texturePath.Length == 0) continue; // not found

					// load the assets
					font.bmFont = AssetDatabase.LoadAssetAtPath(bmFontPath, typeof(UnityEngine.Object));
					font.texture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
				}

				string targetDir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(target));

				// create necessary assets
				if (needFontData) 
				{
					string srcPath = AssetDatabase.GetAssetPath(font.data);
					string destPath = AssetDatabase.GenerateUniqueAssetPath(GetCopyAtTargetPath(platformName, targetDir, srcPath));
					AssetDatabase.CopyAsset(srcPath, destPath);
					AssetDatabase.Refresh();
					font.data = AssetDatabase.LoadAssetAtPath(destPath, typeof(tk2dFontData)) as tk2dFontData;
				}
				if (needFontEditorData) 
				{
					string srcPath = AssetDatabase.GetAssetPath(font.editorData);
					string destPath = AssetDatabase.GenerateUniqueAssetPath(GetCopyAtTargetPath(platformName, targetDir, srcPath));
					AssetDatabase.CopyAsset(srcPath, destPath);
					AssetDatabase.Refresh();
					font.editorData = AssetDatabase.LoadAssetAtPath(destPath, typeof(tk2dFont)) as tk2dFont;
				}
				
				if (font.editorData.bmFont != font.bmFont ||
					font.editorData.texture != font.texture ||
					font.editorData.data != font.data)
				{
					font.editorData.bmFont = font.bmFont;
					font.editorData.texture = font.texture;
					font.editorData.data = font.data;
					EditorUtility.SetDirty(font.editorData);
				}
			}


			proxy.CopyToTarget(target);
		}

		static string GetCopyAtTargetPath(string platformName, string targetDir, string srcPath)
		{
			string filename = System.IO.Path.GetFileNameWithoutExtension(srcPath);
			string ext = System.IO.Path.GetExtension(srcPath);
			string targetPath = targetDir + "/" + filename + "@" + platformName + ext;
			string destPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
			return destPath;
		}

		static void LogNotFoundError(string platformName, string assetName, string assetType)
		{
			Debug.LogError(string.Format("Unable to find platform specific {0} '{1}' for platform '{2}'", assetType, assetName, platformName));
		}

	}
}